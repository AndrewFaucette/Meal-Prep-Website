// Validation returns only known fields. Raw request bodies never become MongoDB queries.
export class HttpError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
  }
}

function requireValue(condition, message) {
  if (!condition) throw new HttpError(400, message);
}

function object(value, allowed, label) {
  requireValue(value !== null && typeof value === 'object' && !Array.isArray(value), `${label} must be an object.`);
  for (const key of Object.keys(value)) {
    requireValue(allowed.includes(key), `${label}.${key} is not an allowed field.`);
  }
}

function text(value, label, maximum = 200, allowEmpty = false) {
  requireValue(typeof value === 'string', `${label} must be a string.`);
  const result = value.trim();
  requireValue((allowEmpty || result.length > 0) && result.length <= maximum,
    `${label} must contain ${allowEmpty ? '0' : '1'}-${maximum} characters.`);
  return result;
}

function money(value, label) {
  requireValue(typeof value === 'number' && Number.isFinite(value) && value >= 0 && value <= 1000000,
    `${label} must be a number between 0 and 1000000 dollars.`);
  requireValue(Math.abs(value * 100 - Math.round(value * 100)) < 0.000001,
    `${label} must have at most two decimal places.`);
  return value;
}

function boolean(value, label) {
  requireValue(typeof value === 'boolean', `${label} must be true or false.`);
  return value;
}

function userId(value) {
  // Links are optional for compatibility with the week JSON already discussed.
  if (value === null) return null;
  requireValue(typeof value === 'string' && /^[a-f\d]{24}$/i.test(value), 'userId must be a 24-character MongoDB ID or null.');
  return value.toLowerCase();
}

function budget(value, label) {
  object(value, ['amount'], label);
  return { amount: money(value.amount, `${label}.amount`) };
}

function meal(value, label) {
  object(value, ['name', 'cost', 'ingredients', 'nutritionalInformation'], label);
  return {
    name: text(value.name ?? '', `${label}.name`, 200, true),
    cost: money(value.cost, `${label}.cost`),
    ingredients: text(value.ingredients, `${label}.ingredients`, 5000, true),
    nutritionalInformation: text(value.nutritionalInformation, `${label}.nutritionalInformation`, 5000, true)
  };
}

const dayNames = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

function days(value) {
  requireValue(Array.isArray(value) && value.length === 7, 'days must contain exactly seven days.');
  const result = value.map((day, index) => {
    const label = `days[${index}]`;
    object(day, ['name', 'planned', 'spent', 'plannedMeal'], label);
    requireValue(dayNames.includes(day.name), `${label}.name must be a full English day name.`);
    return {
      name: day.name,
      planned: budget(day.planned, `${label}.planned`),
      spent: budget(day.spent, `${label}.spent`),
      plannedMeal: meal(day.plannedMeal, `${label}.plannedMeal`)
    };
  });
  requireValue(new Set(result.map(day => day.name)).size === 7, 'Each day name must appear exactly once.');
  return dayNames.map(name => result.find(day => day.name === name));
}

function password(value) {
  requireValue(typeof value === 'string' && value.length >= 12 && value.length <= 128,
    'password must contain 12-128 characters.');
  return value; // Password whitespace is significant; never trim it.
}

const schemas = {
  users: {
    required: ['username', 'email', 'password'],
    fields: {
      username: value => text(value, 'username', 80).toLowerCase(),
      email: value => {
        const result = text(value, 'email', 254).toLowerCase();
        requireValue(/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(result), 'email must be a valid email address.');
        return result;
      },
      password
    },
    defaults: {}
  },
  weeks: {
    required: ['name', 'days'],
    fields: {
      name: value => text(value, 'name'),
      planComplete: value => boolean(value, 'planComplete'),
      userId,
      days
    },
    defaults: { planComplete: false, userId: null }
  },
  recipes: {
    required: ['name', 'cost', 'ingredients', 'nutritionalInformation'],
    fields: {
      name: value => text(value, 'name'),
      cost: value => money(value, 'cost'),
      ingredients: value => text(value, 'ingredients', 5000, true),
      nutritionalInformation: value => text(value, 'nutritionalInformation', 5000, true),
      description: value => text(value, 'description', 5000, true),
      instructions: value => text(value, 'instructions', 10000, true),
      author: value => text(value, 'author', 100, true),
      category: value => {
        requireValue(['quick', 'budget', 'vegetarian', 'other'].includes(value), 'category must be quick, budget, vegetarian, or other.');
        return value;
      },
      time: value => text(value, 'time', 80, true),
      userId
    },
    defaults: { description: '', instructions: '', author: '', category: 'other', time: '', userId: null }
  }
};

export function validate(resource, input, partial = false) {
  const schema = schemas[resource];
  object(input, Object.keys(schema.fields), 'body');
  requireValue(Object.keys(input).length > 0, 'Request body cannot be empty.');
  if (!partial) {
    for (const key of schema.required) {
      requireValue(Object.hasOwn(input, key), `${key} is required.`);
    }
  }
  const result = partial ? {} : { ...schema.defaults };
  for (const [key, value] of Object.entries(input)) {
    result[key] = schema.fields[key](value);
  }
  return result;
}
