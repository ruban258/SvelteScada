import {WebSocketLink } from 'apollo-link-ws';
import {split } from 'apollo-link';
import {HttpLink} from 'apollo-link-http';
import {getMainDefinition } from 'apollo-utilities';
import {ApolloClient} from 'apollo-client';
import {InMemoryCache} from 'apollo-cache-inmemory';

const wsLink = new WebSocketLink({
    uri: 'ws://localhost:5000/graphql',
    options: {
      reconnect: true
    }
  });
const httpLink = new HttpLink({
uri: 'http://localhost:5000/grapql/',
fetchOptions: {
  mode: 'no-cors',
},
});

// The split function takes three parameters:
//
// * A function that's called for each operation to execute
// * The Link to use for an operation if the function returns a "truthy" value
// * The Link to use for an operation if the function returns a "falsy" value
const splitLink = split(
    ({ query }) => {
      const definition = getMainDefinition(query);
      return (
        definition.kind === 'OperationDefinition' &&
        definition.operation === 'subscription'
      );
    },
    wsLink,
    httpLink,
  );


export default new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache()
});




