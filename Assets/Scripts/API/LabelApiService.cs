using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace API
{
    /// <summary>
    /// Provides coroutine-based HTTP methods for persisting and loading ArtifactLabels
    /// via the Spring Boot backend (base URL: http://localhost:8080).
    /// All methods return IEnumerators to be started by the caller via StartCoroutine.
    /// </summary>
    public static class LabelApiService
    {
        private const string BaseUrl = "http://localhost:8080";

        // ──────────────────────────────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// POST /api/labels/{artifactId}
        /// Sends the label's metadata and its texture PNG as a multipart form upload.
        /// The backend sets textureUrl and (on first save) version; the response DTO is
        /// passed back so the caller can update its local label fields.
        /// </summary>
        public static IEnumerator UploadLabel(
            string artifactId,
            string metadataJson,
            byte[] pngBytes,
            Action<ArtifactLabelDto> onSuccess,
            Action<string> onError)
        {
            string url = $"{BaseUrl}/api/labels/{artifactId}";

            List<IMultipartFormSection> form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("metadata", metadataJson, "application/json"),
                new MultipartFormFileSection("texture", pngBytes, "texture.png", "image/png")
            };

            using UnityWebRequest request = UnityWebRequest.Post(url, form);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ArtifactLabelDto dto = JsonUtility.FromJson<ArtifactLabelDto>(request.downloadHandler.text);
                onSuccess?.Invoke(dto);
            }
            else
            {
                string errorMsg = $"UploadLabel failed [{request.responseCode}]: {request.error} — {request.downloadHandler?.text}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }

        /// <summary>
        /// GET /api/labels/{artifactId}
        /// Fetches all labels for the given artifact from the backend.
        /// Returns a list of ArtifactLabelDto (including textureUrl and version).
        /// Note: JsonUtility cannot deserialize a root JSON array, so the backend response
        /// is wrapped manually before parsing.
        /// </summary>
        public static IEnumerator FetchAllLabels(
            string artifactId,
            Action<List<ArtifactLabelDto>> onSuccess,
            Action<string> onError)
        {
            string url = $"{BaseUrl}/api/labels/{artifactId}";

            using UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // JsonUtility cannot parse a root JSON array — wrap it in an object first.
                string wrapped = $"{{\"items\":{request.downloadHandler.text}}}";
                ArtifactLabelDtoList dtoList = JsonUtility.FromJson<ArtifactLabelDtoList>(wrapped);

                List<ArtifactLabelDto> result = new List<ArtifactLabelDto>(dtoList.items ?? Array.Empty<ArtifactLabelDto>());
                onSuccess?.Invoke(result);
            }
            else
            {
                string errorMsg = $"FetchAllLabels failed [{request.responseCode}]: {request.error}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }

        /// <summary>
        /// GET /api/textures/{filename}
        /// Downloads the PNG mask for a label and returns it as a Texture2D.
        /// The filename is extracted from the textureUrl stored on the label (e.g. "/textures/abc.png" → "abc.png").
        /// </summary>
        public static IEnumerator FetchTexture(
            string textureUrl,
            Action<Texture2D> onSuccess,
            Action<string> onError)
        {
            // textureUrl from the backend looks like "/textures/<uuid>.png"
            string filename = System.IO.Path.GetFileName(textureUrl);
            string url = $"{BaseUrl}/api/textures/{filename}";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                onSuccess?.Invoke(texture);
            }
            else
            {
                string errorMsg = $"FetchTexture failed [{request.responseCode}] for '{textureUrl}': {request.error}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }

        /// <summary>
        /// DELETE /api/labels/{id}
        /// Deletes the label from the backend.
        /// </summary>
        public static IEnumerator DeleteLabel(
            string labelId,
            Action onSuccess,
            Action<string> onError)
        {
            string url = $"{BaseUrl}/api/labels/{labelId}";

            using UnityWebRequest request = UnityWebRequest.Delete(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke();
            }
            else
            {
                string errorMsg = $"DeleteLabel failed [{request.responseCode}]: {request.error}";
                Debug.LogError(errorMsg);
                onError?.Invoke(errorMsg);
            }
        }
    }
}
