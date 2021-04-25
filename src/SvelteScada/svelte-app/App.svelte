<script>
  import { query, subscribe } from "svelte-apollo";
	import { setClient } from "svelte-apollo";
  import {allTags} from './Stores.js';
  import {client, READ_ALL_TAGS, READ_TAG} from './GraphQLClient';
  
  setClient(client);

  const tag = subscribe(READ_TAG, {fetchPolicy: "cache-and-network",});
  const tags = query(READ_ALL_TAGS, {fetchPolicy: "cache-and-network",});

  tags.subscribe(event => {
    if(event.loading){    
      console.log("loading")}
    else if (event.data){
    let localtags = event.data.tags;  
    allTags.set(localtags);
  }});

  //tags.subscribe(event => console.log(JSON.stringify(event.data)));
  tag.subscribe(event =>{
      if($tags.loading){    
      console.log("loading")}
    else if ($tags.data && $allTags){    
      $allTags = $allTags.map(p => p.tagName === event.data.onTagUpdated.tagName ? { ...p, value: event.data.onTagUpdated.value }: p);
    }});  
</script>
<main>
  <nav class="bg-blue-900 shadow-lg">
    <div class="container mx-auto">
      <div class="sm:flex">
        <a href class="text-white text-3xl font-bold p-3">APP LOGO</a>       
        <!-- Menus -->
        <div class="ml-55 mt-4">
          <ul class="text-white sm:self-center text-xl">
            <li class="sm:inline-block">
              <a href class="p-3 hover:text-red-900">About</a>
            </li>
            <li class="sm:inline-block">
              <a href class="p-3 hover:text-red-900">Services</a>
            </li>
            <li class="sm:inline-block">
              <a href class="p-3 hover:text-red-900">Blog</a>
            </li>
            <li class="sm:inline-block">
              <a href class="p-3 hover:text-red-900">Contact</a>
            </li>
          </ul>
        </div>  
      </div>
    </div>
  </nav>
</main>


<style global lang="postcss">
  @tailwind base;
  @tailwind components;
  @tailwind utilities;
</style>	
