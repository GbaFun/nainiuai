namespace IdleApi.Model.Exceptions
{
    public class IdleException : Exception
    {
        public IdleException(string msg, RoleModel role = null)
        {
            Role = role;
            Msg = msg;
        }
        public RoleModel Role { get; set; }

        public string Msg { get; set; }
    }
}
