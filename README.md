
# Pixel API

End-users of this API will be embedding an image into their pages to track visits to their sites.


## Features

- Captures requests to that image and stores visitors asynchronously in the plain text log for further analysis.


## Run Locally

Clone the repository
```bash
git clone https://github.com/jpjcorreia/pixel-api.git
```

Navigate to the project directory
```bash
cd pixel-api
```

Build and run using Docker
```bash
docker-compose up --build
```


## Usage/Examples

```bash
curl --location 'http://localhost:8080/track'
```

