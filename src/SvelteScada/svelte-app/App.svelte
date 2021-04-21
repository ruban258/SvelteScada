<script>
  import {
		ApolloClient,
		InMemoryCache,
		split,
	} from "@apollo/client";
	import { getMainDefinition } from "@apollo/client/utilities";
	import { WebSocketLink } from "@apollo/link-ws";
  import { gql } from "@apollo/client";
  import { query, subscribe } from "svelte-apollo";
	import { setClient } from "svelte-apollo";
  import { createHttpLink } from 'apollo-link-http';

  
  const wsLink = new WebSocketLink({
    uri: "wss://localhost:5001/graphql",
    options: {
      reconnect: true
    }
  });
const httpLink = createHttpLink({
uri: 'https://localhost:5001/graphql/',
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
setClient(client);
const tag = subscribe(READ_TAG, {
        fetchPolicy: "cache-and-network",
    });
const tags = query(READ_ALL_TAGS, {fetchPolicy: "cache-and-network",});

    function getTaskIdsOfProject() {
        
        console.log($tag.data.onTagUpdated.value)
    }

</script>

<main>
  <!--
    <h1>{$tags.data.tags[0].tagName}</h1>
<button on:click={getTaskIdsOfProject}>clickme</button>
  -->
  {#if $tag.loading}
  <h1>Waiting for new books...</h1>
  {:else if $tag.data}
  New Book: {$tag.data.onTagUpdated.value}
  {/if}

  {#if $tags.loading}
  Loading...
{:else if $tags.error}
  Error: {$tags.error.message}
{:else}
  {#each $tags.data.tags as tag}
    {tag.tagName} by {tag.value}
  {/each}
{/if}
                  
</main>

<style>
	
</style>