using ArticleManagementAPI.Entities;
using ArticleManagementAPI.Models;
using ArticleManagementAPI.Services;
using AutoMapper;
using AzureBlobStorage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArticleManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SasTokenGeneratorController(ILogger<SasTokenGeneratorController> _logger
            , IHttpClientFactory _httpClientFactory
            , IConfiguration _configuration
            , IBlobStorageService _blobService) : ControllerBase
    {
        [HttpGet]
        [Route("BlobSasToken/{containerName}/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GenerateSasToken([FromRoute] string containerName, [FromRoute] string fileName)
        {
            _logger.LogInformation("Get SAS Token");
            using (var httpClient = _httpClientFactory.CreateClient("SasGeneratorService"))
            {
                var resourceUri = $"{_configuration["SasTokenService:GetSasToken"]}?fileName={fileName}&containerName={containerName}";
                var httpResponseMessage = await httpClient.GetAsync(resourceUri);
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var res = await httpResponseMessage.Content.ReadAsStringAsync();
                    return Ok(res);
                }
                return Problem("Error while obtaining sas token");
            }
        }

        [HttpGet]
        [Route("BlobSasToken2/{containerName}/{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GenerateSasToken2([FromRoute] string containerName, [FromRoute] string fileName)
        {
            _logger.LogInformation("Get SAS Token");
            var url = await _blobService.CreateUserDelegationSasAsync(containerName
                , fileName
                , 1
                , null
                , Azure.Storage.Sas.BlobContainerSasPermissions.Write);
            return Ok(url);
        }
    }
}
