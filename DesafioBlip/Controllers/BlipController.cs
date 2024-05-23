
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestEase;
using System.Net;

namespace DesafioBlip.Controllers
{
    /// <summary>
    /// Redirect user controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BlipController : ControllerBase
    {
        #region Private Fields

        private readonly IBlipFacade _repositoryFacade;

        #endregion Private Fields

        /// <summary>
        /// Inject <paramref name="facade"/>.
        /// </summary>
        public BlipController(IBlipFacade facade)
        {
            _repositoryFacade = facade;
        }

        /// <summary>
        /// Get the oldest repositories from Blip´s GitHub
        /// </summary>

        [HttpGet]
        public IActionResult GetRepositoryAsync()
        {
            var apiResult = _repositoryFacade.GetRepositoriesAsync();
            if (apiResult == null)
            {
                return NotFound();
            }
            return Ok(apiResult);
        }

    }

    public class BlipFacade : IBlipFacade
    {
        #region Private Fields

        private readonly IBlipServices _services;

        #endregion Private Fields

        #region Public Constructors
        public BlipFacade(IBlipServices services)
        {
            _services = services;
        }

        #endregion Public Constructors


        public List<UsuarioModel> GetRepositoriesAsync()
        {
            var getResponseListFromRepositories = _services.GetGitHubUrl();

            return getResponseListFromRepositories;
        }
    }

    public class UsuarioModel
    {
        public string RepositoryName { get; set; }
        public string RepositoryDescription { get; set; }
        public Uri RepositoryAvatar { get; set; }
    }

    public class RepositoryResponse
    {
        public string Name { get; set; }
        public UsuarioModel Owner { get; set; }
        public string Description { get; set; }
    }

    public class BlipServices : IBlipServices
    {
        #region Private Fields

        private const int NUMBER_OF_REPOSITORIES_TO_GET_IN_REQUEST = 5;

        #endregion Private Fields

        #region Public Methods
        public List<UsuarioModel> GetGitHubUrl()
        {
            var gitUrl = "https://api.github.com/orgs/takenet/repos";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(gitUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "request";

            var message = "";
            using (var response = httpWebRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                message = reader.ReadToEnd();
            }

            List<UsuarioModel> information = SelectList(message);

            return information;
        }
        private List<UsuarioModel> SelectList(string message)
        {
            var dataList = new List<UsuarioModel>();
            try
            {
                var jsonConvert = JsonConvert.DeserializeObject<List<RepositoryResponse>>(message);
                var json = jsonConvert.Take(NUMBER_OF_REPOSITORIES_TO_GET_IN_REQUEST).ToList();

                foreach (var item in json.Where(item => item != null))
                {
                    dataList.Add(new UsuarioModel
                    {
                        RepositoryName = item.Name,
                        RepositoryDescription = item.Description,
                        RepositoryAvatar = item.Owner.RepositoryAvatar
                    });
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dataList;
        }

        #endregion Public Methods
    }

    public interface IBlipServices
    {

        #region Public Methods
        /// <summary>
        /// Takes information from Blip´s oldest repositories.
        /// </summary>
        [Get]
        List<UsuarioModel> GetGitHubUrl();

        #endregion Public Methods
    }
    public interface IBlipFacade
    {
        #region Public Methods

        /// <summary>
        /// Takes information from Blip´s oldest reporitories.
        /// </summary>
        List<UsuarioModel> GetRepositoriesAsync();

        #endregion Public Methods
    }
}
