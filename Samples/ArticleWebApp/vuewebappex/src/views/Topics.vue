<script setup lang="ts">
import TopicTable from '../components/TopicTable.vue'
import SasToken from '@/components/SasToken.vue';
import { ref } from 'vue';

var message = 'Topic management'
const value = ref(null);
</script>

<template>
  <div class="greetings">
    <h1 class="green">{{ message }}</h1>
    <h3 class="green">Topics</h3>
    <TopicTable />
    <h3 class="green new-topic">SAS Token </h3>
    <SasToken/>
  </div>
</template>

<style scoped>
h3 {
  font-size: 1.2rem;
}

</style>
