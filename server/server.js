import { MongoClient } from 'mongodb';
import { createApp } from './app.js';

const { MONGODB_URI, MONGODB_DB = 'mealPrep', PORT = '3000' } = process.env;
if (!MONGODB_URI) {
  console.error('MONGODB_URI is missing. Copy .env.example to .env and enter your Atlas connection string.');
  process.exit(1);
}
if (!/^\d+$/.test(PORT) || Number(PORT) < 1 || Number(PORT) > 65535) {
  console.error('PORT must be an integer between 1 and 65535.');
  process.exit(1);
}

const client = new MongoClient(MONGODB_URI, { serverSelectionTimeoutMS: 10000 });
let server;

async function stop() {
  const timeout = setTimeout(() => process.exit(1), 10000);
  timeout.unref();
  if (server) await new Promise(resolve => server.close(resolve));
  await client.close();
  clearTimeout(timeout);
}

try {
  await client.connect();
  const db = client.db(MONGODB_DB);
  await db.command({ ping: 1 });
  // Sparse indexes let old documents without these fields remain readable.
  // API-created usernames and emails are normalized to lowercase.
  await db.collection('users').createIndex({ username: 1 }, { unique: true, sparse: true });
  await db.collection('users').createIndex({ email: 1 }, { unique: true, sparse: true });
  await db.collection('weeks').createIndex({ userId: 1, _id: 1 });
  await db.collection('recipes').createIndex({ userId: 1, _id: 1 });
  const app = createApp(db);
  server = app.listen(Number(PORT), '127.0.0.1');
  await new Promise((resolve, reject) => {
    server.once('listening', resolve);
    server.once('error', reject);
  });
  console.log(`Connected to MongoDB. Server ready at http://127.0.0.1:${PORT}`);
  process.once('SIGINT', () => { stop().catch(() => process.exit(1)); });
  process.once('SIGTERM', () => { stop().catch(() => process.exit(1)); });
} catch (error) {
  console.error('Startup failed. Check Atlas credentials, network access, and duplicate user records. Error type:', error.name);
  await client.close();
  process.exitCode = 1;
}
