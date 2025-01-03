import HttpService from "./HttpService";
import type TopicResponseModel from "@/models/TopicResponseModel";
import type TopicCreationRequestModel from "@/models/TopicCreationRequestModel";

export default class TopicService 
{
    httpService: HttpService = new HttpService();

    getTopicsData() {
        return [
            {
                code: 'f230fh0g3',
                name: 'Bamboo Watch',
                description: 'Product Description',
                isActive: false
            },
            {
                code: 'nvklal433',
                name: 'Black Watch',
                description: 'Product Description',
                isActive: true
            }
        ];
    }

    getTopicsDataMini() {
        return Promise.resolve(this.getTopicsData().slice(0, 5));
    }
    
    async getTopicsFromAPI(): Promise<TopicResponseModel[]>{
        return await this.httpService.get<TopicResponseModel[]>("Topic/Topics")
    }
    async addNewTopic(model: TopicCreationRequestModel){
        return await this.httpService.post("Topic/Topic", model)
    }
};

