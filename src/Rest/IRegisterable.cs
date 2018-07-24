using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PipServices.Net.Rest
{
    public interface IRegisterable
    {
        void Register();
    }
}
