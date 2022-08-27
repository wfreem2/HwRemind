namespace HwRemind.Api.Endpoints.Users.Models
{
    public class User : BaseUser
    {
        public int? loginId { get; set; }
    }

    public class BaseUser
    {
        public BaseUser() { }
        public BaseUser(User user)
        {
            id = user.id;
            firstName = user.firstName;
            lastName = user.lastName;
            schoolName = user.schoolName;
        }

        public int? id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string schoolName { get; set; }
    }
}
