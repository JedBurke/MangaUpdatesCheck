﻿using HtmlAgilityPack;
using MangaUpdatesCheck.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MangaUpdatesCheck
{
    public class SeriesData : ISeriesData
    {
        static readonly string ATTRIBUTE_HREF = "href";

        private bool LazyParsing;
        private HtmlDocument ParsedDocument;
        private HtmlNode ParsedDocumentRootNode;

        private RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        
        private string _documentContent = string.Empty;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private bool? _isCompleted;
        private bool? _isFullyScanlated;
        private string _seriesType = string.Empty;

        private string _publisher = string.Empty;
        private double _year = -1;

        private Uri _authorLink = null;
        private Uri _illustratorLink = null;
        private Uri _publisherLink = null;


        public SeriesData()
            : this(string.Empty)
        {
        }

        public SeriesData(string content)
            : this(content, true)
        {
        }

        public static SeriesData Empty;

        public SeriesData(string documentContent, bool lazyParsing)
        {
            // Parse content.
            this._documentContent = documentContent;
            this.LazyParsing = lazyParsing;

            if (!string.IsNullOrWhiteSpace(documentContent))
            {
                Parse(this._documentContent);
            }
        }

        /// <summary>
        /// Gets the series title.
        /// </summary>
        public string Title
        {
            get
            {
                if (string.IsNullOrEmpty(this._title))
                {
                    this._title = GetTitle();
                }

                return this._title;

            }
        }

        /// <summary>
        /// Gets the series description1.
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrEmpty(this._description))
                {
                    this._description = GetDescription();
                }

                return this._description;
            }
        }

        /// <summary>
        /// Gets whether the series has been completed in its origin country.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                if (!this._isCompleted.HasValue)
                {
                    this._isCompleted = GetIsCompleted();
                }

                return this._isCompleted.Value;
            }
        }

        /// <summary>
        /// Gets whether the series has been fully scanlated.
        /// </summary>
        public bool IsFullyScanlated
        {
            get
            {
                if (!this._isFullyScanlated.HasValue)
                {
                    this._isFullyScanlated = GetIsFullyScanlated();
                }

                return this._isFullyScanlated.Value;
            }
        }

        /// <summary>
        /// Gets whether the series has been licensed in English.
        /// </summary>
        public bool IsLicensed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the series' author.
        /// </summary>
        public string Author
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the series' illustrator or artist.
        /// </summary>
        public string Illustrator
        {
            get;
            set;
        }

        public string SeriesType
        {
            get {
                if (string.IsNullOrEmpty(_seriesType))
                {
                    _seriesType = GetSeriesType();
                }

                return _seriesType;
            }
        }

        public string Publisher
        {
            get {
                if (string.IsNullOrWhiteSpace(this._publisher))
                {
                    this._publisher = GetPublisher();
                }

                return this._publisher;
            }
        }

        public double Year
        {
            get {
                if (this._year == -1)
                {
                    this._year = GetYear();
                }

                return this._year;
            }
        }

        public Uri AuthorLink
        {
            get {
                if (this._authorLink == null)
                {
                    this._authorLink = GetAuthorLink();
                }

                return this._authorLink;
            }
        }

        public Uri IllustratorLink
        {
            get {
                if (this._illustratorLink == null)
                {
                    this._illustratorLink = GetIllustratorLink();
                }

                return this._illustratorLink;
            }
        }

        public Uri PublisherLink
        {
            get {
                if (this._publisherLink == null)
                {
                    this._publisherLink = GetPublisherLink();
                }

                return this._publisherLink;
            }
        }

        private string GetTitle()
        {
            var xPathTitle = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[1]/span[1]";
            var titleNode = ParsedDocument.DocumentNode.SelectSingleNode(xPathTitle);

            if (titleNode != null)
            {
                var title = titleNode.InnerText;

                if (!string.IsNullOrWhiteSpace(title))
                {
                    title = title.Trim();
                }

                return title;
            }

            return string.Empty;
        }

        private string GetDescription()
        {
            var xPathDescription = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[2]";
            var descriptionNode = ParsedDocument.DocumentNode.SelectSingleNode(xPathDescription);

            if (descriptionNode != null)
            {
                var description = descriptionNode.InnerText;

                if (!string.IsNullOrWhiteSpace(description))
                {
                    description = description.Trim();
                }

                return description;
            }

            return string.Empty;
        }

        private bool GetIsCompleted()
        {
            string
                XpathStatusInCountry = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[13]/b",
                XpathStatusInCountryComplete = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[14]";

            /// Check the series' status in its country.
            var country = ParsedDocument.DocumentNode.SelectSingleNode(XpathStatusInCountry);

            if (country != null && Regex.IsMatch(country.InnerText, Resources.ScrapeStatusInCountry, regexOptions))
            {
                // If the node has been found, check if it is complete.
                var status = ParsedDocument.DocumentNode.SelectSingleNode(XpathStatusInCountryComplete);

                if (status != null)
                {
                    return Regex.IsMatch(status.InnerText, Resources.ScrapeStatusInCountryComplete, regexOptions);
                }
            }

            return false;
        }

        private bool GetIsFullyScanlated()
        {
            string
                XpathScanlationStatus = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[15]/b",
                XpathCompletelyScanlated = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[16]";

            // Check the scanlation status of the series.
            var scanlated = ParsedDocument.DocumentNode.SelectSingleNode(XpathScanlationStatus);

            if (scanlated != null && Regex.IsMatch(scanlated.InnerText, Resources.ScrapeScanlatedText, regexOptions))
            {
                var status = ParsedDocument.DocumentNode.SelectSingleNode(XpathCompletelyScanlated);

                if (status != null)
                {
                    return Regex.IsMatch(status.InnerText, Resources.ScrapeScanlatedConfirmText, regexOptions);
                }
            }

            return false;
        }

        private string GetSeriesType()
        {
            string
                xPathTypeCategory = "/html/body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[3]/b",
                xPathType = "//body/div/table/tr[3]/td/table//td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[3]/div/div[4]";

            var typeCategoryNode = ParsedDocument.DocumentNode.SelectSingleNode(xPathTypeCategory);

            if (typeCategoryNode != null && Regex.IsMatch(typeCategoryNode.InnerText, Resources.ScrapeTypeText, regexOptions))
            {
                var typeNode = ParsedDocument.DocumentNode.SelectSingleNode(xPathType);

                if (typeNode != null && !string.IsNullOrWhiteSpace(typeNode.InnerText))
                {
                    return typeNode.InnerText.Trim();
                }
            }
            
            return string.Empty;
        }

        private string GetPublisher()
        {
            string
                xPathPublisherCategory = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[17]/b",
                xPathPublisher = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[18]/a/u";

            return ScrapeInformation(xPathPublisherCategory, xPathPublisher, Resources.ScrapePublisherHeader);

        }

        private double GetYear()
        {
            double publishedYear = 0;

            string 
                xPathYearCategory = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[15]/b",
                xPathYear = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[16]";

            string value = ScrapeInformation(xPathYearCategory, xPathYear, Resources.ScrapeYearText);

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (!double.TryParse(value, out publishedYear))
                {
                    // Signify that it has failed.
                    publishedYear = -1;
                }
            }

            return publishedYear;
        }

        private Uri GetAuthorLink()
        {
            string 
                xPathAuthorHeader = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[11]/b",
                xPathAuthor = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[12]/a";

            string value = ScrapeAttributeInformation(xPathAuthorHeader, xPathAuthor, Resources.ScrapeAuthorHeader, ATTRIBUTE_HREF);
            
            return value != null ? new Uri(value) : null;
        }

        private Uri GetIllustratorLink()
        {
            string
                xPathIllustratorHeader = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[13]/b",
                xPathIllustrator = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[14]/a";

            string value = ScrapeAttributeInformation(xPathIllustratorHeader, xPathIllustrator, Resources.ScrapeIllustratorHeader, ATTRIBUTE_HREF);

            return value != null ? new Uri(value) : null;
        }

        private Uri GetPublisherLink()
        {
            string
                xPathPublisherHeader = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[17]/b",
                xPathPublisher = "//body/div/table/tr[3]/td/table/tr/td[2]/table/tr[2]/td/table[2]/tr/td/div[1]/div[4]/div/div[18]/a";

            string value = ScrapeAttributeInformation(xPathPublisherHeader, xPathPublisher, Resources.ScrapePublisherHeader, ATTRIBUTE_HREF);

            return value != null ? new Uri(value) : null;
        }
        
        private void Parse(string documentContent)
        {
            if (string.IsNullOrEmpty(documentContent))
            {
                throw new ArgumentNullException();
            }

            ParsedDocument = new HtmlDocument();
            ParsedDocument.LoadHtml(documentContent);

            ParsedDocumentRootNode = ParsedDocument.DocumentNode;

            if (!LazyParsing)
            {
                this._title = GetTitle();
                this._isCompleted = GetIsCompleted();
                this._isFullyScanlated = GetIsFullyScanlated();
                this._description = GetDescription();
                this._seriesType = GetSeriesType();
                this._year = GetYear();
                this._publisher = GetPublisher();

                this._authorLink = GetAuthorLink();
                this._illustratorLink = GetIllustratorLink();
                this._publisherLink = GetPublisherLink();
            }
        }

        private string ScrapeInformation(string categoryXpath, string valueXpath, string expectedCategoryText)
        {
            var categoryNode = ParsedDocumentRootNode.SelectSingleNode(categoryXpath);
            if (categoryNode != null && Regex.IsMatch(categoryNode.InnerText, expectedCategoryText, regexOptions))
            {
                var valueNode = ParsedDocumentRootNode.SelectSingleNode(valueXpath);

                if (valueNode != null && !string.IsNullOrWhiteSpace(valueNode.InnerText))
                {
                    string value = valueNode.InnerText;
                    value = value.Trim();

                    return value;
                }
            }

            return string.Empty;
        }

        private HtmlNode ScrapeInformationAsHtmlNode(string categoryXpath, string valueXpath, string expectedCategoryText)
        {
            var categoryNode = ParsedDocumentRootNode.SelectSingleNode(categoryXpath);
            if (categoryNode != null && Regex.IsMatch(categoryNode.InnerText, Regex.Escape(expectedCategoryText), regexOptions))
            {
                var valueNode = ParsedDocumentRootNode.SelectSingleNode(valueXpath);

                return valueNode;
            }

            return null;
        }

        private string ScrapeAttributeInformation(string categoryXpath, string valueXpath, string expectedCategoryText, string attribute)
        {
            var linkNode = ScrapeInformationAsHtmlNode(categoryXpath, valueXpath, expectedCategoryText);
            if (linkNode != null && linkNode.HasAttributes)
            {
                string value = linkNode.Attributes[attribute].Value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    value = value.Trim();
                    return value;
                }

            }

            return null;

        }

    }
}
