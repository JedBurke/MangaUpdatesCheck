﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization.Json;

namespace MangaUpdatesCheck
{
    public class Series
    {
        public async Task<ISeriesData> SearchAsync(string series)
        {
            return await Task.Run(() => Search(series));
        }

        public ISeriesData Search(string series)
        {
            return Search(series, SearchResultOutput.Json);
        }

        public ISeriesData Search(string series, SearchResultOutput outputType)
        {
            byte[] response = null;
            string seriesSanitized = null;
            Serialization.IResults results = null;
            System.Collections.Specialized.NameValueCollection param = null;

            try
            {
                seriesSanitized = Helpers.Search.FormatParameters(series);

                param = new NameValueCollection();
                param.Add("act", "series");
                param.Add("stype", "title");
                param.Add("search", seriesSanitized);
                param.Add("x", "0");
                param.Add("y", "0");

                // Todo: Check, probably not best practice to convert an enum to string like this.
                param.Add("output", outputType.ToString().ToLower());

                response = PerformQuery(new Uri(Properties.Resources.SeriesSearchUri), param);

                using (var serializeResults = new SerializeResults())
                {
                    results = serializeResults.Serialize(response);
                }

                // Todo: Do a proper word-boundary comparison.
                var item = results.Items.FirstOrDefault(i => i.Title == series);
                var info = FetchSeriesData(item.Id);

                return info;
            }
            finally
            {
                response = null;
                seriesSanitized = null;

                // Todo: Dispose.
                results = null;

                if (param != null)
                {
                    param.Clear();
                    param = null;
                }
            }
        }

        private byte[] PerformQuery(Uri queryUri, NameValueCollection parameters)
        {
            byte[] response = null;
            int tries = 0;

            while (tries++ < 5)
            {
                try
                {
                    using (var request = new System.Net.WebClient())
                    {
                        response = request.UploadValues(new Uri(Properties.Resources.SeriesSearchUri), parameters);
                        break;
                    }

                }
                catch (System.Net.WebException)
                {
                    // Log exception and retry.
                }
            }

            return response;

        }

        public ISeriesData FetchSeriesData(int id)
        {
            return FetchSeriesData(new Uri(string.Format(Properties.Resources.SeriesUriFormat, id)));
        }

        public ISeriesData FetchSeriesData(Uri uri)
        {
            ISeriesData parsedContent = null;
            int tries = 0;

            while (tries++ < 5)
            {
                try
                {
                    using (var client = new System.Net.WebClient())
                    {
                        string content = client.DownloadString(uri);

                        parsedContent = SeriesDataParser.Parse(content);
                    }
                }
                catch (WebException)
                {
                    // Log exception and retry.

                }
            }

            return parsedContent;

        }

    }
}
