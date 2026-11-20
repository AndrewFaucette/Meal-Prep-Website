import { ObjectId } from 'mongodb';

// A deliberately small test double. Actual Atlas behavior needs the README smoke check.
export function testDatabase() {
  const collections = new Map();
  return {
    async command() { return { ok: 1 }; },
    collection(name) {
      if (collections.has(name)) return collections.get(name);
      const rows = new Map();
      const key = id => id.toString();
      function duplicate(document) {
        if (name !== 'users') return;
        for (const row of rows.values()) {
          if (key(row._id) !== key(document._id) && (row.username === document.username || row.email === document.email)) {
            const error = new Error('Duplicate');
            error.code = 11000;
            throw error;
          }
        }
      }
      const collection = {
        async insertOne(document) {
          document._id ??= new ObjectId();
          duplicate(document);
          rows.set(key(document._id), { ...document });
          return { insertedId: document._id };
        },
        async findOne({ _id }) { return rows.get(key(_id)) ?? null; },
        find(filter) {
          let data = [...rows.values()].filter(row => Object.entries(filter).every(([field, value]) => row[field] === value));
          return {
            sort() { data.sort((a, b) => key(a._id).localeCompare(key(b._id))); return this; },
            skip(count) { data = data.slice(count); return this; },
            limit(count) { data = data.slice(0, count); return this; },
            async toArray() { return data; }
          };
        },
        async replaceOne({ _id }, document) {
          if (!rows.has(key(_id))) return { matchedCount: 0 };
          duplicate({ ...document, _id });
          rows.set(key(_id), { ...document, _id });
          return { matchedCount: 1 };
        },
        async findOneAndUpdate({ _id }, update) {
          const current = rows.get(key(_id));
          if (!current) return null;
          const result = { ...current, ...update.$set };
          for (const field of Object.keys(update.$unset ?? {})) delete result[field];
          duplicate(result);
          rows.set(key(_id), result);
          return result;
        },
        async deleteOne({ _id }) { return { deletedCount: rows.delete(key(_id)) ? 1 : 0 }; }
      };
      collections.set(name, collection);
      return collection;
    }
  };
}

