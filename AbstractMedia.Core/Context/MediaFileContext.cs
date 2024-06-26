﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AbstractMedia.Core.Models;

namespace AbstractMedia.Core.Context;

public class MediaFileContext : IMediaContext
{
    private readonly string _filePath;

    public MediaFileContext(string filePath)
    {
        _filePath = filePath;

        // Create the directory and file if they do not exist
        var directory = Path.GetDirectoryName(_filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Close();
        }

        Media = LoadDataFromFile() ?? new List<Media>();
    }

    public void AddMedia(Media media)
    {
        Media.Add(media);
    }

    public void DeleteMedia(Media media)
    {
        Media.Remove(media);
    }

    public Media GetMediaById(int id)
    {
        return Media.FirstOrDefault(m => m.Id == id);
    }

    public List<Media> Media { get; set; }

    public void SaveChanges()
    {
        SaveDataToFile(Media);
    }

    public void UpdateMedia(Media media)
    {
        var existingMedia = Media.FirstOrDefault(m => m.Id == media.Id);
        if (existingMedia != null)
        {
            var index = Media.IndexOf(existingMedia);
            Media[index] = media;
        }
    }

    private List<Media> LoadDataFromFile()
    {
        var mediaList = new List<Media>();
        var lines = File.ReadAllLines(_filePath).Skip(1);

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            var type = parts[0];
            var id = int.Parse(parts[1]);
            var title = parts[2];
            var season = string.IsNullOrEmpty(parts[3]) ? 0 : int.Parse(parts[3]);
            var episode = string.IsNullOrEmpty(parts[4]) ? 0 : int.Parse(parts[4]);
            var writers = string.IsNullOrEmpty(parts[5]) ? null : parts[5].Split('|');
            var format = parts[6];
            var length = string.IsNullOrEmpty(parts[7]) ? 0 : int.Parse(parts[7]);
            var regions = string.IsNullOrEmpty(parts[8]) ? null : parts[8].Split('|').Select(int.Parse).ToArray();
            var genres = string.IsNullOrEmpty(parts[9]) ? null : parts[9].Split('|');

            switch (type)
            {
                case "Movie":
                    mediaList.Add(new Movie(id, title, genres));
                    break;
                case "Show":
                    mediaList.Add(new Show(id, title, season, episode, writers));
                    break;
                case "Video":
                    mediaList.Add(new Video(id, title, format, length, regions));
                    break;
                default:
                    throw new Exception("Unknown media type");
            }
        }

        return mediaList;
    }

    // TODO: Implement the SaveDataToFile method
    private void SaveDataToFile(List<Media> media)
    {
        // Create a list to store the lines of data to be written to the file
        var lines = new List<string>();

        // Add the header line
        lines.Add("Type,Id,Title,Season,Episode,Writers,Format,Length,Regions,Genres");

        // Loop through each media item in the 'media' list
        foreach (var item in media)
        {
            // Convert the media item's properties to a string format
            string line;
            switch (item)
            {
                case Movie movie:
                    line = $"Movie,{movie.Id},{movie.Title},,,,{string.Join("|", movie.Genres)},,,";
                    break;
                case Show show:
                    line = $"Show,{show.Id},{show.Title},{show.Season},{show.Episode},{string.Join("|", show.Writers)},,,";
                    break;
                case Video video:
                    line = $"Video,{video.Id},{video.Title},,,,{video.Format},{video.Length},{string.Join("|", video.Regions)},";
                    break;
                default:
                    throw new Exception("Unknown media type");
            }

            // Add the formatted string to the list of lines
            lines.Add(line);
        }

        // Write the lines to the CSV file
        File.WriteAllLines(_filePath, lines);
    }

}
