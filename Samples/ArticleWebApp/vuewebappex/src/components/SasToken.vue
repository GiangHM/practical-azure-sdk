<script setup lang="ts">
import InputText from 'primevue/inputtext';
import Button from 'primevue/button';
import { ref } from 'vue';
import SasTokenService from '@/services/SasTokenService';

const fileName = defineModel({ type: String, default: '' })
var sasToken = ref("");
let sasTokenService = new SasTokenService();

async function getBlobSasToken() {
    var res = await sasTokenService.getBlobSasToken("TestFiles", fileName.value)
    sasToken.value = res;
}
</script>

<template>
    <div class="card flex justify-center">
        <div class="flex flex-col gap-2">
            <label for="code">FileName: </label>
            <InputText id="code" v-model="fileName" type="text" size="small" />
        </div>    
        <div>
            <label>{{sasToken}}</label>
        </div>    
    </div>
    <div>
        <Button label="Submit" @click="getBlobSasToken()"/>
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