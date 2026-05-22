namespace Wcs.WebApi.OpenApi;

public static class OpenApiDocumentFactory
{
    public static object Create(IConfiguration configuration)
    {
        var title = configuration["OpenApi:Title"] ?? "WCS Web API";
        var version = configuration["OpenApi:Version"] ?? "v1";

        return new
        {
            openapi = "3.0.1",
            info = new
            {
                title,
                version,
                description = "Backend entry point for the WCS front-end/back-end separation migration."
            },
            paths = new Dictionary<string, object>
            {
                ["/health"] = new
                {
                    get = new
                    {
                        tags = new[] { "System" },
                        summary = "Health check endpoint",
                        responses = new Dictionary<string, object>
                        {
                            ["200"] = new
                            {
                                description = "Service is healthy"
                            }
                        }
                    }
                },
                ["/api/system/summary"] = new
                {
                    get = new
                    {
                        tags = new[] { "System" },
                        summary = "Return migration foundation metadata",
                        responses = new Dictionary<string, object>
                        {
                            ["200"] = new
                            {
                                description = "Service metadata"
                            }
                        }
                    }
                }
            }
        };
    }

    public static string CreateSwaggerUiHtml(string openApiUrl)
    {
        return $"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>WCS Web API</title>
  <link rel="stylesheet" href="https://unpkg.com/swagger-ui-dist/swagger-ui.css" />
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://unpkg.com/swagger-ui-dist/swagger-ui-bundle.js"></script>
  <script>
    window.onload = () => {{
      window.ui = SwaggerUIBundle({{
        url: '{openApiUrl}',
        dom_id: '#swagger-ui'
      }});
    }};
  </script>
</body>
</html>
""";
    }
}
