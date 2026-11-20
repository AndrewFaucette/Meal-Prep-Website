import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { request as httpRequest } from 'node:http';
import { ObjectId } from 'mongodb';
import { createApp } from './app.js';

import { testDatabase } from './testing/test-database.js';

test('HTTP CRUD, validation, and password protection', async t => {
  const db = testDatabase();
  const server = createApp(db).listen(0, '127.0.0.1');
  await new Promise(resolve => server.once('listening', resolve));
  t.after(() => new Promise(resolve => server.close(resolve)));
  const base = `http://127.0.0.1:${server.address().port}`;
  async function request(method, path, body) {
    const response = await fetch(`${base}${path}`, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body)
    });
    return { status: response.status, body: response.status === 204 ? null : await response.json() };
  }
  const samples = {};
  for (const [resource, filename] of [['users', 'user'], ['weeks', 'week'], ['recipes', 'recipe']]) {
    samples[resource] = JSON.parse(await readFile(new URL(`./examples/${filename}.json`, import.meta.url), 'utf8'));
  }

  await t.test('complete CRUD for each collection, including PUT and PATCH', async () => {
    for (const resource of ['users', 'weeks', 'recipes']) {
      const sample = samples[resource];
      const created = await request('POST', `/api/${resource}`, sample);
      assert.equal(created.status, 201);
      assert.match(created.body._id, /^[a-f0-9]{24}$/);
      const path = `/api/${resource}/${created.body._id}`;
      const read = await request('GET', path);
      assert.equal(read.status, 200);
      assert.equal(read.body._id, created.body._id);
      const listed = await request('GET', `/api/${resource}?page=1&limit=1`);
      assert.equal(listed.body.data.length, 1);
      const changes = resource === 'users' ? { username: 'changed' } : { name: 'Changed' };
      const patched = await request('PATCH', path, changes);
      assert.equal(patched.status, 200);
      assert.equal(patched.body[Object.keys(changes)[0]], Object.values(changes)[0]);
      const replaced = await request('PUT', path, sample);
      assert.equal(replaced.status, 200);
      assert.equal(replaced.body._id, created.body._id);
      if (resource === 'users') {
        for (const result of [created.body, read.body, listed.body.data[0], patched.body, replaced.body]) {
          assert.equal(result.password, undefined);
          assert.equal(result.passwordHash, undefined);
        }
        const stored = await db.collection('users').findOne({ _id: new ObjectId(created.body._id) });
        assert.match(stored.passwordHash, /^scrypt:[a-f0-9]{32}:[a-f0-9]{128}$/);
        assert.equal(stored.password, undefined);
      }
      assert.equal((await request('DELETE', path)).status, 204);
      for (const method of ['GET', 'DELETE', 'PUT', 'PATCH']) {
        assert.equal((await request(method, path, ['PUT', 'PATCH'].includes(method) ? sample : undefined)).status, 404);
      }
    }
  });

  await t.test('bad IDs, invalid JSON, operators, and incomplete data are rejected', async () => {
    assert.equal((await request('GET', '/api/weeks/invalid')).status, 400);
    assert.equal((await request('GET', '/api/weeks?limit=200')).status, 400);
    assert.equal((await request('GET', '/api/weeks?page=-1')).status, 400);
    assert.equal((await request('GET', '/api/weeks?userId[$ne]=x')).status, 400);
    assert.equal((await request('POST', '/api/weeks', {})).status, 400);
    assert.equal((await request('POST', '/api/weeks', [samples.weeks])).status, 400);
    assert.equal((await request('POST', '/api/users', { ...samples.users, email: { $ne: null } })).status, 400);
    assert.equal((await request('POST', '/api/recipes', { ...samples.recipes, cost: -2 })).status, 400);
    assert.equal((await request('POST', '/api/recipes', { ...samples.recipes, cost: 1.234 })).status, 400);
    assert.equal((await request('POST', '/api/weeks', { ...samples.weeks, days: samples.weeks.days.slice(1) })).status, 400);
    const duplicateDays = structuredClone(samples.weeks);
    duplicateDays.days[1].name = 'Monday';
    assert.equal((await request('POST', '/api/weeks', duplicateDays)).status, 400);
    assert.equal((await request('PATCH', `/api/weeks/${new ObjectId()}`, { $set: { name: 'injection' } })).status, 400);
    const malformed = await fetch(`${base}/api/weeks`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{broken' });
    assert.equal(malformed.status, 400);
  });

  await t.test('duplicate users return 409; legacy plaintext is hidden and removed on password update', async () => {
    const user = await request('POST', '/api/users', samples.users);
    assert.equal((await request('POST', '/api/users', samples.users)).status, 409);
    const legacy = { username: 'legacy', email: 'legacy@example.com', password: 'old-plaintext' };
    const { insertedId } = await db.collection('users').insertOne(legacy);
    assert.equal((await request('GET', `/api/users/${insertedId}`)).body.password, undefined);
    const updated = await request('PATCH', `/api/users/${insertedId}`, { password: 'ReplacementPassword123' });
    assert.equal(updated.status, 200);
    assert.equal(updated.body.passwordHash, undefined);
    assert.equal((await db.collection('users').findOne({ _id: insertedId })).password, undefined);
    await request('DELETE', `/api/users/${user.body._id}`);
    await request('DELETE', `/api/users/${insertedId}`);
  });

  await t.test('user filtering, full day replacement, health, and missing routes', async () => {
    const userId = new ObjectId().toHexString();
    const first = await request('POST', '/api/weeks', { ...samples.weeks, userId });
    await request('POST', '/api/weeks', samples.weeks);
    const filtered = await request('GET', `/api/weeks?userId=${userId}`);
    assert.equal(filtered.body.data.length, 1);
    const changed = structuredClone(samples.weeks.days);
    changed[0].plannedMeal.name = 'New meal';
    const patched = await request('PATCH', `/api/weeks/${first.body._id}`, { days: changed });
    assert.equal(patched.body.days[0].plannedMeal.name, 'New meal');
    assert.equal(patched.body.days.length, 7);
    assert.equal((await request('GET', '/api/health')).body.status, 'ok');
    assert.equal((await request('GET', '/api/missing')).status, 404);
  });

  await t.test('local browser protections and request format errors', async () => {
    for (const headers of [
      { Origin: 'https://attacker.example' },
      { Origin: 'null' },
      { Host: 'attacker.example' },
      { 'Sec-Fetch-Site': 'cross-site' }
    ]) {
      // Use raw HTTP so fetch cannot replace Host / browser metadata headers.
      const response = await new Promise((resolve, reject) => {
        const request = httpRequest(`${base}/api/recipes`, { headers }, res => {
          let body = '';
          res.setEncoding('utf8');
          res.on('data', chunk => { body += chunk; });
          res.on('end', () => resolve({ status: res.statusCode, body: JSON.parse(body) }));
        });
        request.on('error', reject);
        request.end();
      });
      assert.equal(response.status, 403, JSON.stringify(headers));
      assert.equal(typeof response.body.error, 'string');
    }
    const sameOrigin = await fetch(`${base}/api/recipes`, { headers: { Origin: base } });
    assert.equal(sameOrigin.status, 200);
    assert.equal(sameOrigin.headers.get('cache-control'), 'no-store');
    for (const contentType of ['text/plain', 'application/x-www-form-urlencoded', 'application/json; charset=iso-8859-1']) {
      const response = await fetch(`${base}/api/recipes`, {
        method: 'POST', headers: { 'Content-Type': contentType }, body: '{}'
      });
      assert.equal(response.status, 415);
    }
    const large = await fetch(`${base}/api/recipes`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: 'x'.repeat(270000) })
    });
    assert.equal(large.status, 413);
  });
});
