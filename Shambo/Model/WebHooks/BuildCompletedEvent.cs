using System;
using System.Collections.Generic;

namespace Shambo.Model.WebHooks
{
   [Serializable]
   public class Message
   {
      public string Text { get; set; }
      public string Html { get; set; }
      public string Markdown { get; set; }
   }

   [Serializable]
   public class DetailedMessage
   {
      public string Text { get; set; }
      public string Html { get; set; }
      public string Markdown { get; set; }
   }

   [Serializable]
   public class Drop
   {
      public string Location { get; set; }
      public string Type { get; set; }
      public string Url { get; set; }
      public string DownloadUrl { get; set; }
   }

   [Serializable]
   public class Log
   {
      public string Type { get; set; }
      public string Url { get; set; }
      public string DownloadUrl { get; set; }
   }

   [Serializable]
   public class LastChangedBy
   {
      public string Id { get; set; }
      public string DisplayName { get; set; }
      public string UniqueName { get; set; }
      public string Url { get; set; }
      public string ImageUrl { get; set; }
   }

   [Serializable]
   public class Definition
   {
      public int BatchSize { get; set; }
      public string TriggerType { get; set; }
      public string DefinitionType { get; set; }
      public int Id { get; set; }
      public string Name { get; set; }
      public string Url { get; set; }
   }

   [Serializable]
   public class Queue
   {
      public string QueueType { get; set; }
      public int Id { get; set; }
      public string Name { get; set; }
      public string Url { get; set; }
   }

   [Serializable]
   public class RequestedFor
   {
      public string Id { get; set; }
      public string DisplayName { get; set; }
      public string UniqueName { get; set; }
      public string Url { get; set; }
      public string ImageUrl { get; set; }
   }

   [Serializable]
   public class Request
   {
      public int Id { get; set; }
      public string Url { get; set; }
      public RequestedFor RequestedFor { get; set; }
   }

   [Serializable]
   public class Resource
   {
      public string Uri { get; set; }
      public int Id { get; set; }
      public string BuildNumber { get; set; }
      public string Url { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime FinishTime { get; set; }
      public string Reason { get; set; }
      public string Status { get; set; }
      public string DropLocation { get; set; }
      public Drop Drop { get; set; }
      public Log Log { get; set; }
      public string SourceGetVersion { get; set; }
      public LastChangedBy LastChangedBy { get; set; }
      public bool RetainIndefinitely { get; set; }
      public bool HasDiagnostics { get; set; }
      public Definition Definition { get; set; }
      public Queue Queue { get; set; }
      public List<Request> Requests { get; set; }
   }

   [Serializable]
   public class Collection
   {
      public string Id { get; set; }
   }

   [Serializable]
   public class Account
   {
      public string Id { get; set; }
   }

   [Serializable]
   public class Project
   {
      public string Id { get; set; }
   }

   [Serializable]
   public class ResourceContainers
   {
      public Collection Collection { get; set; }
      public Account Account { get; set; }
      public Project Project { get; set; }
   }

   [Serializable]
   public class BuildCompletedEvent
   {
      public string Id { get; set; }
      public string EventType { get; set; }
      public string PublisherId { get; set; }
      public string Scope { get; set; }
      public Message Message { get; set; }
      public DetailedMessage DetailedMessage { get; set; }
      public Resource Resource { get; set; }
      public string ResourceVersion { get; set; }
      public ResourceContainers ResourceContainers { get; set; }
      public DateTime CreatedDate { get; set; }
   }
}