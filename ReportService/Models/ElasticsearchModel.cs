using System.Text.Json.Serialization;

namespace ReportService.Models
{
    /// <summary>
    /// Represents a medical note associated with a patient.
    /// </summary>
    public class MedicalNoteModel
    {
        public string NoteId { get; set; }
        public int PatientId { get; set; }

        public string Note { get; set; }
        public DateTime Date { get; set; }
        public List<string> HighlightedTriggers { get; set; }
    }

    /// <summary>
    /// Represents a term used as a trigger in the system.
    /// </summary>
    public class TriggerTerm
    {
        public string Term { get; set; }
        public string Category { get; set; }

        public TriggerTerm(string term, string category)
        {
            Term = term;
            Category = category;
        }
    }

    /// <summary>
    /// Represents the response from an Elasticsearch search query.
    /// </summary>
    /// <typeparam name="T">The type of the source object in the search results.</typeparam>
    public class ElasticsearchSearchResponse<T>
    {
        [JsonPropertyName("hits")]
        public HitsContainer<T> Hits { get; set; }
    }

    /// <summary>
    /// Represents a container for search hits in Elasticsearch.
    /// </summary>
    /// <typeparam name="T">The type of the source object in the hits.</typeparam>
    public class HitsContainer<T>
    {
        [JsonPropertyName("hits")]
        public List<Hit<T>> HitList { get; set; }
    }

    /// <summary>
    /// Represents a single search hit in Elasticsearch.
    /// </summary>
    /// <typeparam name="T">The type of the source object in the hit.</typeparam>
    public class Hit<T>
    {
        [JsonPropertyName("_source")]
        public T Source { get; set; }

        [JsonPropertyName("highlight")]
        public Dictionary<string, List<string>> Highlight { get; set; }
    }
}
