using System;
using System.Collections.Generic;

namespace I_Fly.Models
{
    [Serializable]
    public class Post
    {
        public int Id { get; set; }
        public string Description { get; set; }

        //Id of the planet this post belongs to
        public int Id_Planet { get; set; }

        //If this post is a rest area => so far same prices so used to filter and concatenate in one post.
        public bool Is_Rest_Area { get; set; }

        //If "illegal" post
        public bool Is_Pirate { get; set; }

        public byte[] Picture { get; set; } = new byte[0];
    }

    public static class Post_Extensions
    {
        public static int Get_Id(this Post p_input, List<Post> p_posts)
        {
            for (var i = 0; i < p_posts.Count; i++)
            {
                if (p_posts[i].Description == p_input.Description)
                {
                    return p_posts[i].Id;
                }
            }

            throw new Exception("Post not found " + p_input.Description);
        }

        public static int Get_Post_By_Id(this Post p_input, List<Post> p_posts)
        {
            for (var i = 0; i < p_posts.Count; i++)
            {
                if (p_posts[i].Description == p_input.Description)
                {
                    return p_posts[i].Id;
                }
            }

            throw new Exception("Post not found " + p_input.Description);
        }
    }
}
