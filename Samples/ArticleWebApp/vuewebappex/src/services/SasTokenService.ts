import HttpService from "./HttpService";

export default class SasTokenService 
{
    httpService: HttpService = new HttpService();

    
    async getBlobSasToken(containerName: string, fileName: string): Promise<string>{
        var route = `SasTokenGenerator/BlobSasToken/${containerName}/${fileName}`;
        return await this.httpService.get<string>(route);
    }
};

