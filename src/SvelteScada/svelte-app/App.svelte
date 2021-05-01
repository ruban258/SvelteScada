<script>
  import { query, subscribe } from "svelte-apollo";
	import { setClient } from "svelte-apollo";
  import {allTags} from './Stores.js';
  import {client, READ_ALL_TAGS, READ_TAG} from './GraphQLClient';
  import Navbar from './Navbar.svelte';
  import Gauge from './Gauge.svelte';
  
  setClient(client);

  const tag = subscribe(READ_TAG, {fetchPolicy: "cache-and-network",});
  const tags = query(READ_ALL_TAGS, {fetchPolicy: "cache-and-network",});

  tags.subscribe(event => {
    if(event.loading){    
      console.log("loading")}
    else if (event.data){
    let localtags = event.data.tags;  
    localtags.forEach(element => {
      let tagName = element.tagName;
      let value = element.value;
      $allTags[tagName] = {value}
    });    
  }});
  
  tag.subscribe(event =>{
      if($tags.loading){    
      console.log("loading")}
    else if ($tags.data && $allTags){    
      $allTags[event.data.onTagUpdated.tagName].value = event.data.onTagUpdated.value;      
    }});  
</script>
<main class="bg-gray-100">
  <Navbar></Navbar>
  <div class="mt-7 text-5xl flex-col content-center">
    <Gauge value ={10}/>
    <h1>Temperature</h1>
  </div>
  
</main>


<style global lang="postcss">
  @tailwind base;
  @tailwind components;
  @tailwind utilities;
</style>	
