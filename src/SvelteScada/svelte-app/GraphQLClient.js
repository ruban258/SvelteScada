import { ApolloClient,from,InMemoryCache,split,} from "@apollo/client";
import { getMainDefinition } from "@apollo/client/utilities";
import { WebSocketLink } from "@apollo/link-ws";
import { gql } from "@apollo/client";
import { createHttpLink } from 'apollo-link-http';



const wsLink = new WebSocketLink({
uri: "wss://localhost:5001/graphql",
options: {
    reconnect: true
}
});
const httpLink = createHttpLink({uri: 'https://localhost:5001/graphql/'});

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
const client = new ApolloClient({
  link: splitLink,
  cache: new InMemoryCache()
});
       
const READ_ALL_TAGS = gql`query{
  tags{
    tagName
    value
  }}`;

const READ_TAG = gql`subscription{
    onTagUpdated{
    tagName
    value
  }
}`;


export {client, READ_ALL_TAGS, READ_TAG};