// TEST ONLY: no MongoDB, no persistent storage. Never use for real application data.
import { createApp } from '../app.js';
import { testDatabase } from './test-database.js';

const server = createApp(testDatabase()).listen(0, '127.0.0.1', () => {
  console.log(`TEST FIXTURE ONLY: http://127.0.0.1:${server.address().port}`);
});
for (const signal of ['SIGINT', 'SIGTERM']) {
  process.once(signal, () => server.close());
}
