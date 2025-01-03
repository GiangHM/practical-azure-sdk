using ArticleManagementAPI.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArticleManagementAPI.Services
{
    public interface ITopicTableService
    {
        Task<TopicEntity> AddEntity(TopicEntity entity);
        Task<IEnumerable<TopicEntity>> GetAllData();
    }
}
