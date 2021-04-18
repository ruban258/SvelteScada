<script>
	import client from './graphql-client.js';
    import gql from 'graphql-tag';

    let tag ={ TagName:"", Value:""};
   
    const READ_ALL_TAGS = gql`query{
    tags{
    tagName
    value
    }
    }`;

    const READ_TAG = gql`subscription{
    onTagUpdated{
    tagName
    value
  }
}`;

const getAllCountries = () => {
    client
      .query({
        query: READ_ALL_TAGS
      })
      .then((res) => {
        tags = res.data;
        console.log(tags);
      })
      .catch((err) => console.log(err));
  };

  let tags = client.subscribe({
        query: READ_TAG
      });
</script>

<main>
	<table>
        <tbody>
            {#each $tags.data.onTagUpdated as t}
                <tr>
                    <td>{t.TagName}</td>
                    <td>{t.Value}</td>
                </tr>
            {/each}
        </tbody>

    </table>
</main>

<style>
	
</style>