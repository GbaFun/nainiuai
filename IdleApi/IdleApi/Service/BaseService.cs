using IdleApi.Service.Interface;
using IdleApi.Wrap;

namespace IdleApi.Service
{
    public class BaseService : IService
    {
        private BroWindow _win { get; set; }

        public UserModel User { get; set; }

        public BaseService(BroWindow win)
        {
            _win = win;
            User = win.User;
        }

        public BroWindow GetWin()
        {
            return _win;
        }
    }
}
