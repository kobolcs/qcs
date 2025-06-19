import { Verifier } from '@pact-foundation/pact';
import { server } from './server.js';
import path from 'path';
import { jest } from '@jest/globals';

// Set a longer timeout for Pact verification
jest.setTimeout(30000);

describe('Pact Verification', () => {
  it('validates the expectations of the WebAppConsumer', () => {
    const opts = {
      provider: 'UserAPIProvider',
      providerBaseUrl: 'http://localhost:8081',
      pactUrls: [path.resolve(process.cwd(), '..', 'pacts', 'WebAppConsumer-UserAPIProvider.json')],
      stateHandlers: {
        'a user with ID 1 exists': () => {
          // This is where you would set up your DB for the test
          return Promise.resolve('User 1 is available');
        },
      },
    };

    return new Verifier(opts)
      .verifyProvider()
      .then(() => {
        console.log('Pact verification complete!');
      })
      .finally(() => {
        server.close();
      });
  });
});
