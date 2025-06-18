import { PactV3, MatchersV3 } from '@pact-foundation/pact';
import { ApiClient } from './apiClient.js';
import path from 'path';

const { like } = MatchersV3;

const provider = new PactV3({
  consumer: 'WebAppConsumer',
  provider: 'UserAPIProvider',
  dir: path.resolve(process.cwd(), '..', 'pacts'), // Output pacts to a shared parent directory
  logLevel: 'warn',
});

describe('API Pact test', () => {
  it('returns a user object for ID 1', () => {
    // 1. Define the expected interaction
    provider
      .given('a user with ID 1 exists')
      .uponReceiving('a request for user 1')
      .withRequest({
        method: 'GET',
        path: '/users/1',
        headers: { Accept: 'application/json' },
      })
      .willRespondWith({
        status: 200,
        headers: { 'Content-Type': 'application/json; charset=utf-8' },
        body: like({
          id: 1,
          name: 'Leanne Graham',
          username: 'Bret',
        }),
      });

    // 2. Run the test against the Pact mock server
    return provider.executeTest(async (mockServer) => {
      const client = new ApiClient(mockServer.url);
      const user = await client.getUser(1);
      expect(user.name).toBe('Leanne Graham');
    });
  });
});
