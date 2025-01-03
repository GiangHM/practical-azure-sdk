<script setup lang="ts">
import DataTable from 'primevue/datatable';
import Column from 'primevue/column';
import { ref, onMounted } from 'vue';
import TopicService  from '@/services/TopicService';
import Button from 'primevue/button';

let topicService = new TopicService();

onMounted(() => {
   topicService.getTopicsFromAPI().then(data => topics.value = data);
});

const topics = ref();

</script>

<template>
    <div class="table-topic">
        <div class="card">
            <DataTable :value="topics" tableStyle="min-width: 40rem">
                <Column field="topicCode" header="Code"></Column>
                <Column field="topicName" header="Name"></Column>
                <Column field="topicDescription" header="Description"></Column>
                <Column field="isActive" header="Is Active"></Column>
            </DataTable>
        </div>
        <div>
            <Button label="Reload" />
        </div>
    </div>
</template>

<style scoped>
.card {
    background: var(--card-bg);
    border: var(--card-border);
    padding: 0.75rem;
    border-radius: 10px;
    margin-bottom: 0.75rem;
}

h1 {
  font-weight: 500;
  font-size: 2.6rem;
  position: relative;
  top: -10px;
}

h3 {
  font-size: 1.2rem;
}

.table-topic{
    margin-bottom: 2rem;
}

</style>
