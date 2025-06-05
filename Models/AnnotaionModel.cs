using NotEdgeForEpubWpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using JsonSubTypes;
using System.Threading.Tasks;
using VersOne.Epub;

namespace NotEdgeForEpubWpf.Models
{
    namespace AnnotationModel
    {
        [JsonConverter(typeof(JsonSubtypes), "type")]
        [JsonSubtypes.KnownSubType(typeof(ProgressionSelector), "ProgressionSelector")]
        [JsonSubtypes.KnownSubType(typeof(RangeOffsetSelector),  "RangeOffsetSelector")]
        [JsonSubtypes.KnownSubType(typeof(XPathSelector),  "XPathSelector")]
        [JsonSubtypes.KnownSubType(typeof(RangeSelector),  "RangeSelector")]
        public abstract class Selector
        {
            [JsonProperty("type")]
            public string Type => GetType().Name;
            //[JsonIgnore]
            //public string Type { get { return GetType().Name; } set { } }
            public string? ToJSON()
            {
                return JsonConvert.SerializeObject(this);
            }
            public static Selector? FromJSON(string json)
            {
                return JsonConvert.DeserializeObject<Selector>(json);
            }
        }
        public class ProgressionSelector: Selector
        {

            //[JsonProperty("type")]
            //public string Type { get { return "ProgressionSelector"; } set {/*for deserialization*/ } }
            [JsonProperty("value")]
            public double Value { get; set; }

            public ProgressionSelector(double value) { Value = value; }
            public ProgressionSelector() { }

        }
        public class RangeOffsetSelector: Selector
        {
            //[JsonProperty("type")]
            //public string Type { get { return "RangeOffsetSelector"; } set {/*for deserialization*/ } }
            [JsonProperty("value")]
            public int Value { get; set; }

        }
        public class XPathSelector: Selector
        {

            //[JsonProperty("type")]
            //public string Type { get { return "XPathSelector"; } set {/*for deserialization*/ } }
            [JsonProperty("value")]
            public string Value { get; set; }
            [JsonProperty("refinedBy")]
            public Selector? RefinedBy { get; set; }


        }
        public class RangeSelector : Selector
        {

            //[JsonProperty("type")]
            //public string Type { get { return "RangeSelector"; } set {/*for deserialization*/ } }
            [JsonProperty("startSelector")]
            public Selector StartSelector { get; set; }
            [JsonProperty("endSelector")]
            public Selector EndSelector { get; set; }

            public string ToJSON()
            {
                return JsonConvert.SerializeObject(this);
            }
            public static RangeSelector? FromJSON(string json)
            {
                return (RangeSelector?)JsonConvert.DeserializeObject<Selector>(json);
            }
        }
        public class AnnoTarget
        {

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("selector")]
            public Selector? Selector { get; set; }
            [JsonProperty("meta")]
            public string? Meta { get; set; }
        }
        public class AnnoBody
        {
            [JsonProperty("type")]
            public string Type { get { return "TextualBody"; } set {/*for deserialization*/ } }
            [JsonProperty("value")]
            public string Value { get; set; } = "";
            [JsonProperty("format")]
            public string? Format { get; set; }

            [JsonProperty("color")]
            public string? Color { get; set; } = "yellow";

            [JsonProperty("highlight")]
            public string? Highlight { get; set; }

            [JsonProperty("language")]
            public string? Language { get; set; }
            [JsonProperty("textDirection")]
            public string? TextDirection { get; set; }
            [JsonProperty("keyword")]
            public string? Keyword { get; set; }

            public AnnoBody(string txt = "")
            {
                this.Value = txt;
            }
            public AnnoBody() { }

        }
        public class AnnoCreator
        {

        }
        public class Annotation
        {
            [JsonProperty("@context")]
            public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
            [JsonProperty("type")]
            public string Type { get { return "Annotation"; } set {/*for deserialization*/ } }
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("motivation")]
            public string? Motivation;

            [JsonProperty("created")]
            public string Creadted { get; set; }

            [JsonProperty("modified")]
            public string? Modified { get; set; }

            [JsonProperty("creator")]
            public AnnoCreator? Creator { get; set; }

            [JsonProperty("target")]
            public AnnoTarget Target { get; set; }

            [JsonProperty("body")]
            public AnnoBody? Body { get; set; }
            public string ToJSON()
            {
                return JsonConvert.SerializeObject(this);
            }
            public static Annotation? FromJSON(string json)
            {
                return JsonConvert.DeserializeObject<Annotation>(json);
            }
        }
        public class AnnotationSetAbout
        {

            [JsonProperty("dc:identifier")]
            public List<string>? Identifier { get; set; }
            [JsonProperty("dc:title")]
            public string? Title { get; set; }
            [JsonProperty("dc:format")]
            public string? Format { get; set; }
            [JsonProperty("dc:publisher")]
            public string? Publisher { get; set; }
            [JsonProperty("dc:creator")]
            public List<string>? Creator { get; set; }
            [JsonProperty("dc:date")]
            public string? Date { get; set; }
            public AnnotationSetAbout(EpubBookRef bookRef)
            {
                var meta = bookRef.Schema.Package.Metadata;
                Identifier = meta.Identifiers.Select(i => i.Identifier).ToList();
                Title = meta.Titles.Select(i => i.Title).FirstOrDefault();
                Format = meta.Formats.Select(i => i.Format).FirstOrDefault();
                Publisher = meta.Publishers.Select(i => i.Publisher).FirstOrDefault();
                Creator = meta.Creators.Select(i => i.Creator).ToList();
                Date = meta.Dates.Select(i => i.Date).FirstOrDefault();
            }
            public AnnotationSetAbout() { }
        }
        public class AnnotationSet
        {

            [JsonProperty("@context")]
            public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
            [JsonProperty("type")]
            public string Type { get { return "AnnotationSet"; } set {/*for deserialization*/ } }
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("about")]
            public AnnotationSetAbout About { get; set; }
            [JsonProperty("generated")]
            public string? Generated { get; set; }
            [JsonProperty("title")]
            public string? Title { get; set; }
            [JsonProperty("items")]
            public List<Annotation> Items { get; set; }

            public AnnotationSet(EpubBookRef bookRef, List<Annotation> annotations)
            {
                Guid uuid = Guid.NewGuid();
                Id = $"urn:uuid:{uuid}";
                About = new AnnotationSetAbout(bookRef);
                Items = annotations;
            }
            public AnnotationSet() { }
        }

        //public class ProgressionSelector
        //{

        //    [JsonPropertyName("type")]
        //    public string Type { get { return "ProgressionSelector"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("value")]
        //    public double Value { get; set; }

        //    public ProgressionSelector(double value) {  Value = value; }
        //    public ProgressionSelector() { }

        //}
        //public class RangeOffsetSelector
        //{
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "RangeOffsetSelector"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("value")]
        //    public int Value { get; set; }

        //}
        //public class XPathSelector
        //{

        //    [JsonPropertyName("type")]
        //    public string Type { get { return "XPathSelector"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("value")]
        //    public string Value { get; set; }
        //    [JsonPropertyName("refinedBy")]
        //    public RangeOffsetSelector? RefinedBy { get; set; }


        //}
        //public class RangeSelector
        //{

        //    [JsonPropertyName("type")]
        //    public string Type { get { return "RangeSelector"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("startSelector")]
        //    public XPathSelector StartSelector { get; set; }
        //    [JsonPropertyName("endSelector")]
        //    public XPathSelector EndSelector { get; set; }

        //    public string ToJSON()
        //    {
        //        return JsonSerializer.Serialize(this);
        //    }
        //    public static RangeSelector? FromJSON(string json)
        //    {
        //        return JsonSerializer.Deserialize<RangeSelector>(json);
        //    }
        //}
        //public class AnnoTarget
        //{

        //    [JsonPropertyName("source")]
        //    public string Source { get; set; }

        //    [JsonPropertyName("selector")]
        //    public RangeSelector? Selector { get; set; }
        //    [JsonPropertyName("meta")]
        //    public string? Meta { get; set; }
        //}
        //public class AnnoBody
        //{
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "TextualBody"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("value")]
        //    public string Value { get; set; } = "";
        //    [JsonPropertyName("format")]
        //    public string? Format { get; set; }

        //    [JsonPropertyName("color")]
        //    public string? Color { get; set; } = "yellow";

        //    [JsonPropertyName("highlight")]
        //    public string? Highlight { get; set; }

        //    [JsonPropertyName("language")]
        //    public string? Language { get; set; }
        //    [JsonPropertyName("textDirection")]
        //    public string? TextDirection { get; set; }
        //    [JsonPropertyName("keyword")]
        //    public string? Keyword { get; set; }

        //    public AnnoBody(string txt="")
        //    {
        //        this.Value=txt;
        //    }
        //    public AnnoBody() { }

        //}
        //public class AnnoCreator
        //{

        //}
        //public class Annotation
        //{
        //    [JsonPropertyName("@context")]
        //    public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "Annotation"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("id")]
        //    public string Id { get; set; }
        //    [JsonPropertyName("motivation")]
        //    public string? Motivation;

        //    [JsonPropertyName("created")]
        //    public string Creadted { get; set; }

        //    [JsonPropertyName("modified")]
        //    public string? Modified { get; set; }

        //    [JsonPropertyName("creator")]
        //    public AnnoCreator? Creator { get; set; }

        //    [JsonPropertyName("target")]
        //    public AnnoTarget Target { get; set; }

        //    [JsonPropertyName("body")]
        //    public AnnoBody? Body { get; set; }
        //    public string ToJSON()
        //    {
        //        return JsonSerializer.Serialize(this);
        //    }
        //    public static Annotation? FromJSON(string json)
        //    {
        //        return JsonSerializer.Deserialize<Annotation>(json);
        //    }
        //}
        //public class AnnotationSetAbout
        //{

        //    [JsonPropertyName("dc:identifier")]
        //    public List<string>? Identifier { get; set; }
        //    [JsonPropertyName("dc:title")]
        //    public string? Title { get; set; }
        //    [JsonPropertyName("dc:format")]
        //    public string? Format { get; set; }
        //    [JsonPropertyName("dc:publisher")]
        //    public string? Publisher { get; set; }
        //    [JsonPropertyName("dc:creator")]
        //    public List<string>? Creator { get; set; }
        //    [JsonPropertyName("dc:date")]
        //    public string? Date { get; set; }
        //    public AnnotationSetAbout(EpubBookRef bookRef)
        //    {
        //        var meta = bookRef.Schema.Package.Metadata;
        //        Identifier = meta.Identifiers.Select(i => i.Identifier).ToList();
        //        Title = meta.Titles.Select(i => i.Title).FirstOrDefault();
        //        Format=meta.Formats.Select(i => i.Format).FirstOrDefault();
        //        Publisher=meta.Publishers.Select(i => i.Publisher).FirstOrDefault();
        //        Creator=meta.Creators.Select(i => i.Creator).ToList();
        //        Date = meta.Dates.Select(i => i.Date).FirstOrDefault();
        //    }
        //    public AnnotationSetAbout() { }
        //}
        //public class AnnotationSet
        //{

        //    [JsonPropertyName("@context")]
        //    public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "AnnotationSet"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("id")]
        //    public string Id { get; set; }
        //    [JsonPropertyName("about")]
        //    public AnnotationSetAbout About { get; set; }
        //    [JsonPropertyName("generated")]
        //    public string? Generated { get; set; }
        //    [JsonPropertyName("title")]
        //    public string? Title { get; set; }
        //    [JsonPropertyName("items")]
        //    public List<Annotation> Items { get; set; }

        //    public AnnotationSet(EpubBookRef bookRef,List<Annotation> annotations)
        //    {
        //        Guid uuid = Guid.NewGuid();
        //        Id = $"urn:uuid:{uuid}";
        //        About = new AnnotationSetAbout(bookRef);
        //        Items = annotations;
        //    }
        //    public AnnotationSet() { }
        //}


        //public class AnnoTargetBookmark
        //{

        //    [JsonPropertyName("source")]
        //    public string Source { get; set; }

        //    [JsonPropertyName("selector")]
        //    public ProgressionSelector? Selector { get; set; }
        //    [JsonPropertyName("meta")]
        //    public string? Meta { get; set; }

        //    public AnnoTargetBookmark(BookProgressData data)
        //    {
        //        Source = data.HtmlPathInEpub;
        //        Selector=new(data.progressInHtml);
        //    }

        //    public AnnoTargetBookmark() { }

        //}

        //public class AnnotationBookmark
        //{
        //    [JsonPropertyName("@context")]
        //    public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "Annotation"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("id")]
        //    public string Id { get; set; }
        //    [JsonPropertyName("motivation")]
        //    public string? Motivation;

        //    [JsonPropertyName("created")]
        //    public string Creadted { get; set; }

        //    [JsonPropertyName("modified")]
        //    public string? Modified { get; set; }

        //    [JsonPropertyName("creator")]
        //    public AnnoCreator? Creator { get; set; }

        //    [JsonPropertyName("target")]
        //    public AnnoTargetBookmark Target { get; set; }

        //    [JsonPropertyName("body")]
        //    public AnnoBody? Body { get; set; }
        //    public string ToJSON()
        //    {
        //        return JsonSerializer.Serialize(this);
        //    }
        //    public static AnnotationBookmark? FromJSON(string json)
        //    {
        //        return JsonSerializer.Deserialize<AnnotationBookmark>(json);
        //    }
        //}

        //public class AnnotationSetBookmark
        //{

        //    [JsonPropertyName("@context")]
        //    public string Context { get { return "http://www.w3.org/ns/anno.jsonld"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("type")]
        //    public string Type { get { return "AnnotationSet"; } set {/*for deserialization*/ } }
        //    [JsonPropertyName("id")]
        //    public string Id { get; set; }
        //    [JsonPropertyName("about")]
        //    public AnnotationSetAbout About { get; set; }
        //    [JsonPropertyName("generated")]
        //    public string? Generated { get; set; }
        //    [JsonPropertyName("title")]
        //    public string? Title { get; set; }
        //    [JsonProperty("items")]
        //    public List<AnnotationBookmark> Items { get; set; }

        //    public AnnotationSetBookmark(EpubBookRef bookRef, List<AnnotationBookmark> annotations)
        //    {
        //        Guid uuid = Guid.NewGuid();
        //        Id = $"urn:uuid:{uuid}";
        //        About = new AnnotationSetAbout(bookRef);
        //        Items = annotations;
        //    }
        //    public AnnotationSetBookmark() { }
        //}

        public class InitTag(string id)
        {
            public string Id { get; set; } = id;
        }
    }
}
