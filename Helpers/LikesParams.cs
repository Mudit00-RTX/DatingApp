namespace API.Helpers
{
    public class LikesParams : PaginationParams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; }  //predicate means that do they want to get the user they have liked or the user that they are liked by
    }
}