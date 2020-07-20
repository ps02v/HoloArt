using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBpedia
{
    [System.Serializable]
    public class DBpediaResult
{
    public bool Success { get; set; } = false;
    public string Artist { get; set; }
    public string Painting { get; set; }

    public string URL { get; set; }

    public string Depiction { get; set; }

    public DBpediaResult(string author, string painting)
    {
        this.Artist = author;
        this.Painting = painting;
        this.Success = true;
    }

    public DBpediaResult(string artist, string title, string depiction, string url)
    {
        this.Artist = artist;
        this.Painting = title;
        this.Depiction = depiction;
        this.URL = url;
        this.Success = true;
    }

    public DBpediaResult()
    {
    }

    public override string ToString()
    {
        if (this.Success)
        {
            string s;
            s = "Artist: " + this.Artist + System.Environment.NewLine;
            s += "Title: " + this.Painting + System.Environment.NewLine;
            s += "Depiction: " + this.Depiction + System.Environment.NewLine;
            s += "URL: " + this.URL;
            return s;
        }
        else
            return "Don't Know.";
    }
}

    [System.Serializable]
    public class Root
    {
        public Head head;
        public Results results;
    }

    [System.Serializable]
    public class Head
    {
        public List<object> link;
        public List<string> vars;
    }

    [System.Serializable]
    public class Results
    {
        public string distinct;
        public string ordered;
        public List<Binding> bindings;
    }

    [System.Serializable]
    public class Binding
    {
        public Artist artist;
        public Title title;
        public Depiction depiction;
        public Url url;

    }

    [System.Serializable]
    public class Artist
    {
        public string type;
        //public string xmllang;
        public string value;
}

    [System.Serializable]
    public class Title
{
        public string type;
        //public string xmllang;
        public string value; 
    }

    [System.Serializable]
    public class Depiction
{
        public string type;
        public string value;
}

    [System.Serializable]
    public class Url
{
        public string type;
        public string value;
}

}