using IdleApi.Scripts;

namespace IdleApi.Service.Interface
{
    public interface ITrade
    {
        Task<bool> AcceptAll(int page = 0, NoticeParser not = null);
    }
}
