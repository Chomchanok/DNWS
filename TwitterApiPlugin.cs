using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
  

        public List<User> GetUsers()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> GetFollowing(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("following");
            string message = request.getRequestByKey("message");
            string[] Url_Path = request.Filename.Split("?"); // create url_path for checking
            if (Url_Path[0] == "users")// if user in users path
            {
                if (request.Method == "GET")// to show list of users
                {
                    string json = JsonConvert.SerializeObject(GetUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")// add users
                {
                    try
                    {
                        Twitter.AddUser(user, password);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("403 User already exists");
                    }
                }
                else if (request.Method == "DELETE") // to delete user
                {
                    Twitter twitter = new Twitter(user);
                    try
                    {
                        twitter.Delete_User(user);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }
            else if (Url_Path[0] == "follow")// if user in follow path
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")// to show list of following
                {
                    string json = JsonConvert.SerializeObject(GetFollowing(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")// add following's user
                {
                    if (Twitter.Check(following))
                    {
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
                else if (request.Method == "DELETE")//delete following's user there are something wrong
                {
                    try
                    {
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }
            else if (Url_Path[0] == "tweets")//if user in tweets path
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")//show tweet of users in timeline 
                {
                    try
                    {
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following")
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                else if (request.Method == "POST")//add new tweet in timeline
                {
                    try
                    {
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            response.type = "application/json";
            return response;
        }
    }
}