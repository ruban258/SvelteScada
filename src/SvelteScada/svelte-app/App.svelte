<script>
  import { ApolloClient,InMemoryCache,split,} from "@apollo/client";
	import { getMainDefinition } from "@apollo/client/utilities";
	import { WebSocketLink } from "@apollo/link-ws";
  import { gql } from "@apollo/client";
  import { query, subscribe } from "svelte-apollo";
	import { setClient } from "svelte-apollo";
  import { createHttpLink } from 'apollo-link-http';
  import { writable } from 'svelte/store';  
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
setClient(client);
let allTags = writable([]);
const tag = subscribe(READ_TAG, {fetchPolicy: "cache-and-network",});
const tags = query(READ_ALL_TAGS, {fetchPolicy: "cache-and-network",});

tags.subscribe(event => {
  allTags.set(event.data); 
});

//tags.subscribe(event => console.log(JSON.stringify(event.data)));
tag.subscribe(event =>{
  if($tags.loading){    
    console.log("loading")}
  else if ($tags.data && $allTags.tags){
    const objIndex = $allTags.tags.findIndex((obj => obj.tagName == event.data.onTagUpdated.tagName));    
    //allTags.update(n => n.tags[objIndex].value = event.data.onTagUpdated.value);
    $allTags.update(n => n.tags[objIndex].value = 100);
    console.log($allTags.tags);
    
  }});

  
</script>
<main>
  {#if $tag.loading}
  <h1>Waiting for new update...</h1>
  {:else if $tag.data}
  Tag Updated: {$tag.data.onTagUpdated.tagName}
  Tag Value: {$tag.data.onTagUpdated.value}
  {/if}

  {#if $tags.loading}
  Loading...
  {:else if $tags.error}
  Error: {$tags.error.message}
  {:else}
  {#each $tags.data.tags as tag}
    {tag.tagName} with {tag.value}
  {/each}
  {/if}
                  
</main>

<style>
	
</style>