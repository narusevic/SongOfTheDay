using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.EntitySql;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime;
using SongOfTheDay.Models;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.AspNet.Identity;

namespace SongOfTheDay.Controllers
{
    public class VideoController : Controller
    {
        public ActionResult VideoLink(string videoLink, string videoInfo)
        {
            if ((videoLink.Contains("www.youtube.com") || videoLink.Contains("www.youtu.be") || videoLink.ToLower().Contains("soundcloud")) && (videoLink.Length >= 27) && videoInfo != null)
            {
                using (var db = new DbConnection())
                {
                    string userId = User.Identity.GetUserId();
                    var client = new WebClient();

                    string name = "";
                    string imageUrl = "";
                    string SCid = "";
                    string SCname = "";
                    string SCimage = "";

                    if (!videoLink.ToLower().Contains("soundcloud"))
                    {

                        var downloadString = client.DownloadString("https://gdata.youtube.com/feeds/api/videos/" + GetVideoApi(videoLink) + "?v=2");
                        var infoList = downloadString.ToList();


                        #region Name

                        var title = downloadString.IndexOf("<media:title type='plain'>");

                        var nameList = new List<char>();

                        for (var i = title; i < infoList.Count; i++)
                        {
                            if (infoList[i] == 62)
                            {
                                for (var x = i + 1; x < infoList.Count; x++)
                                {
                                    if (infoList[x] != 60)
                                    {
                                        nameList.Add(infoList[x]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        for (var i = 0; i < nameList.Count; i++)
                        {
                            name = name + nameList[i];
                        }

                        #endregion

                        #region image

                        var image = downloadString.IndexOf("<media:thumbnail");

                        var imageList = new List<char>();

                        for (var i = image; i < infoList.Count; i++)
                        {
                            if (infoList[i] == 39)
                            {
                                for (var x = i + 1; x < infoList.Count; x++)
                                {
                                    if (infoList[x] != 39)
                                    {
                                        imageList.Add(infoList[x]);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        for (var i = 0; i < imageList.Count; i++)
                        {
                            imageUrl = imageUrl + imageList[i];
                        }

                        #endregion

                        videoLink = ConvertVideoUrl(videoLink);
                    }
                    else
                    {
                        var downloadString = client.DownloadString("http://soundcloud.com/oembed?format=js&url=" + videoLink + "&iframe=true");
                        var infoList = downloadString.ToList();

                        #region SC id

                        var idList = new List<char>();

                        var videoId = downloadString.LastIndexOf("tracks%2F");

                        for (var i = videoId + 9; i < infoList.Count; i++)
                        {
                            if (infoList[i] != 92)
                            {
                                idList.Add(infoList[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = 0; i < idList.Count; i++)
                        {
                            SCid = SCid + idList[i];
                        }
                        #endregion

                        #region SC name

                        var nameList = new List<char>();

                        var videoName = downloadString.LastIndexOf("\"title\":\"");

                        for (var i = videoName + 9; i < infoList.Count; i++)
                        {
                            if (infoList[i] != 34)
                            {
                                nameList.Add(infoList[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = 0; i < nameList.Count; i++)
                        {
                            SCname = SCname + nameList[i];
                        }
                        #endregion

                        #region SC image

                        var imageList = new List<char>();

                        var videoImage = downloadString.LastIndexOf("thumbnail_url\":\"");

                        for (var i = videoImage + 16; i < infoList.Count; i++)
                        {
                            if (infoList[i] != 63)
                            {
                                imageList.Add(infoList[i]);
                            }
                            else
                            {
                                break;
                            }
                        }

                        for (var i = 0; i < imageList.Count; i++)
                        {
                            SCimage = SCimage + imageList[i];
                        }

                        #endregion

                        videoLink = ("https://w.soundcloud.com/player/?url=https%3A//api.soundcloud.com/tracks/" + SCid +
                                     "&amp;auto_play=false&amp;hide_related=false&amp;visual=true");

                    }

                    var newVideo = new Video
                    {
                        Link = videoLink,
                        Name = (name != "") ? name : SCname,
                        Information = videoInfo.Replace("\r\n", Environment.NewLine),
                        Image = (imageUrl != "") ? imageUrl : SCimage,
                        User = db.ApplicationUsers.Find(userId),
                        Date = DateTime.Now
                    };

                    db.Videos.Add(newVideo);

                    db.SaveChanges();
                }
            }
            return RedirectToAction("Admin", "Home");

        }

        public ActionResult RemoveVideo(int id)
        {
            using (var db = new DbConnection())
            {
                var video = db.Videos.FirstOrDefault(x => x.VideoId == id);
                if (video != null)
                {
                    db.Videos.Remove(video);

                    db.SaveChanges();
                }
            }

            return RedirectToAction("Admin", "Home");
        }

        public static string GetVideo()
        {
            using (var db = new DbConnection())
            {
                var video = db.Videos.OrderByDescending(x => x.VideoId).FirstOrDefault();
                if (video != null)
                {
                    return video.Link;
                }
            }

            return null;
        }

        public ActionResult AdminArchive()
        {
            List<Video> videos;

            using (var db = new DbConnection())
            {
                videos = db.Videos.OrderByDescending(x => x.VideoId).ToList();
            }

            videos.ForEach(n => n.Name = (n.Name.Length > 40) ? (n.Name.Substring(0, 37) + "...") : n.Name);

            return PartialView(videos);
        }

        public ActionResult Archive()
        {
            List<Video> videos;

            using (var db = new DbConnection())
            {
                videos = db.Videos.OrderByDescending(x => x.VideoId).ToList();
            }
            //videos.ForEach(n => n.Name = (n.Name.Length > 40) ? (n.Name.Substring(0, 37) + "...") : n.Name);
            videos.ForEach(v => v.Information = v.Information.Replace("\r\n", "<br />"));

            return PartialView(videos);
        }

        public ActionResult EditVideo(int id)
        {
            var video = new Video();

            using (var db = new DbConnection())
            {
                video = db.Videos.Find(id);
            }

            return View(video);
        }

        public ActionResult SaveVideo(int videoId, string editLink, string editName, string editInfo, DateTime editDate)
        {

            using (var db = new DbConnection())
            {
                db.Videos.Find(videoId).Information = editInfo.Replace("\r\n", Environment.NewLine);
                db.Videos.Find(videoId).Name = editName;
                db.Videos.Find(videoId).Link = ConvertVideoUrl(editLink);
                db.Videos.Find(videoId).Date = editDate;

                db.SaveChanges();
            }

            return RedirectToAction("Admin", "Home");
        }

        public string ConvertVideoUrl(string videoLink)
        {
            const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
            const string replacement = "http://www.youtube.com/embed/$1";
            var rgx = new Regex(pattern);

            var videoLinkEmbed = rgx.Replace(videoLink, replacement) + "?hd=1&iv_load_policy=3";

            return videoLinkEmbed;
        }

        public string GetVideoApi(string videoLink)
        {
            const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
            const string replacementId = "$1";
            var rgx = new Regex(pattern);

            var videoUrlId = rgx.Replace(videoLink, replacementId);

            return videoUrlId;
        }


    }
}