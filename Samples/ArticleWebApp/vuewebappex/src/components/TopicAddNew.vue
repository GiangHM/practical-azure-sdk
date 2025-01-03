<script setup lang="ts">
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';

var newtopic: any = {
    code:"",
    name:"",
    description:""
}
</script>

<template>
    <div class="card flex justify-center">
        <div class="flex flex-col gap-2">
            <label for="code">Topic code: </label>
            <InputText id="code" v-model="newtopic.code" type="text" size="small" />
        </div>
        <div class="flex flex-col gap-2">
            <label for="name">Topic name: </label>
            <InputText id="name" v-model="newtopic.name" type="text" size="small"/>
        </div>
        <div class="flex flex-col gap-2">
            <label for="description">Topic desciption: </label>
            <InputText id="description" v-model="newtopic.description" type="text" size="small"/>
        </div>
    </div>
    <div>
        <Button label="Submit" />
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
</style>