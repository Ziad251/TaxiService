var map, datasource, userLocation, finalRoute, mapNode, key;

function GetMap(des) {

    // Get a refrence to our map container div.
    mapNode = document.querySelector('#myMap');
    // Intialize our key variable 
    key = commonSettings;

    //Initialize a map instance.
    //Center map to user's location.
    map = new atlas.Map("myMap", {
        center: [userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]],
        zoom: 12,
        language: 'en-US',
        view: 'Auto',
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: key
        }
    });


    //Wait until the map resources are ready.
    map.events.add('ready', function () {

        //Add the zoom control to the map.
        map.controls.add(new atlas.control.ZoomControl(), {
            position: 'top-right',
        });

        //Create a data source and add it to the map.
        datasource = new atlas.source.DataSource();
        map.sources.add(datasource);


        //Create a symbol layer to render the user's locations, and add it to the map.
        map.layers.add(new atlas.layer.SymbolLayer(datasource, null, {
            iconOptions: {
                image: 'pin-round-darkblue',
                anchor: 'center',
                allowOverlap: true
            },
            textOptions: {
                anchor: "top"
            }
        }));

        //Add a layer for rendering the route lines and have it render under the map labels.
        map.layers.add(new atlas.layer.LineLayer(datasource, null, {
            strokeColor: '#2272B9',
            strokeWidth: 5,
            lineJoin: 'round',
            lineCap: 'round'
        }), 'labels');

        if (userLocation) {
            datasource.add(userLocation);
        }
        RouteToDestination(des);
    });

};

// at startup initialize userLocation and update server with the most recent location
function InitLocation(pos) {

    userLocation =
    {
        type: 'Feature',
        geometry:
        {
            type: 'Point',
            coordinates: [pos.coords.longitude, pos.coords.latitude],
        },

    };
    let initBuffer = [];
    let count = 0;
    const position = {
        latitude: pos.coords.latitude,
        longitude: pos.coords.longitude,
        ts: pos.timestamp
    };

    if (count <= 0) {
        initBuffer.push(position);
        let initblob = new Blob([JSON.stringify(initBuffer)], { type: 'application/json' });
        (async () => {
            const firstTimer = await fetch('/api/position', {
                method: 'post',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: initblob,
            }).catch(error => alert("Unable to send location to server"));
            // let content = await firstTimer.json();
        })();
        initBuffer = [];
        count++;
    }
    return userLocation;
}


// Continiously store and update server with user's location every 2 minutes 
function UpdateLocation () {

    // watch user's location using the GeoLocation API
    navigator.geolocation.watchPosition(pos => {

        const position = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
            ts: pos.timestamp
        };

        let dataBuffer = [];

        // store user's position in an array every 10 seconds
        setInterval(function () {
            dataBuffer.push(position);
        }, 10000);

        userLocation =
        {
            type: 'Feature',
            geometry:
            {
                type: 'Point',
                coordinates: [pos.coords.longitude, pos.coords.latitude],
            },

        };

        // send every 2 mins if there's new data
        setInterval(function () {
            if (dataBuffer.length) {
                let blob = new Blob([JSON.stringify(dataBuffer)], { type: 'application/json' });
                navigator.sendBeacon('/api/position', blob);
                dataBuffer = [];
            }
        }, 1200000);

        return userLocation;
    });
};


function BuildFeatures(usersData) {
    let positions = usersData.map(user => {
        return {
            type: 'Feature',
            geometry: {
                type: 'Point',
                coordinates: [user.longitude, user.latitude],
            }
            , properties: {
                icon: 'shop',
                userId: user.login.username,
                name: user.name.first,
                phone: user.phone,
                email: user.login.username,

            }
        };
    });
};

function RouteToDestination(des) {
    //Remove any previous results from the map.
    datasource.clear();
    let FindNearest = document.createElement('button');
    FindNearest.innerHTML = "Order ride";
    FindNearest.id = "FindNearest";
    let price = document.createElement('button');
    price.innerHTML = "$208.99";
    price.id = "price";
    price.disabled = true;

    // Convert destination address to coordinates
    
    let geoCodingApi = `https://atlas.microsoft.com/search/address/json?subscription-key=${key}&api-version=1.0&language=en-US&`;
    let query = `query=${des}`;

    // make a request to the geocoding api for coordinates to user's destination address
    fetch(geoCodingApi + query, { method: 'get' })
        .then(response => { 
            return response.json()
        })
        .then(json => {
            return json.results;
        })
        .then(data => {

            //Create a Feature for finalRoute
            let finalR =
            {
                type: 'Feature',
                geometry:
                {
                    type: 'Point',
                    coordinates: [data[0].position.lon, data[0].position.lat],
                },
            }
            datasource.add(finalR);

            // Use MapControlCredential to share authentication between a map control and the service module.
            let pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

            //Construct the RouteURL object
            let routeURL = new atlas.service.RouteURL(pipeline);

            // Start and end point input to the routeURL
            let coordinates = [[userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]], [finalR.geometry.coordinates[0], finalR.geometry.coordinates[1]]];

            // Make a search route request
            routeURL.calculateRouteDirections(atlas.service.Aborter.timeout(10000), coordinates).then((directions) => {
                //Get data features from response
                var data = directions.geojson.getFeatures();
                datasource.add(data);
            });

            map.setCamera({
                bounds: atlas.data.BoundingBox.fromData([userLocation, finalR]),
                zoom: 10,
                padding: 15
            });
        }).catch(error => {
            return alert("Invalid destination provided. Please reenter correct address.");
            
        });

    FindNearest.addEventListener('click', () => {
        checkNearby()
    });
    mapNode.appendChild(FindNearest);
    mapNode.appendChild(price);
};

// Send a request to server to find for the driver nearest to user's most recent location
function checkNearby() {
    //Remove  any previous results from the map.
    datasource.clear();
    mapNode.removeChild(price);
    mapNode.removeChild(FindNearest);

    //Construct driver profile card
    let driverProfile = document.createElement('div');
    let driverImage = document.createElement('img');
    let callDriver = document.createElement('button');
    let driverTitle = document.createElement('p');

    // Make a request for the nearest driver
    fetch('/api/nearbyusers', { method: 'get' })
        .then(r => {          
            return r.json()
        }).then(js => {
            return js
        }).then((data) => {
            // Construct driver's profile using the data sent from server
            driverProfile.id = "driverProfile";
            driverTitle.innerHTML = `${data.name.first} ${data.name.last} <br> <br> ETA: 20 mins`; 
            if(data.picture){
                driverImage.src = `${data.picture.thumbnail}`;
            }
            if(data.phone){
                callDriver.innerHTML = `${data.phone}`;
            }
            driverProfile.appendChild(driverTitle);
            driverProfile.appendChild(driverImage);
            driverProfile.appendChild(callDriver);
            mapNode.appendChild(driverProfile);
            
            let destination = data.location.cordinates.geo.Coordinates.Values;

            let dest =
            {
                type: 'Feature',
                geometry:
                {
                    type: 'Point',
                    coordinates: [destination[0], destination[1]],
                },
            }
            datasource.add(dest);

            // Use MapControlCredential to share authentication between a map control and the service module.
            let pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

            //Construct the RouteURL object
            let routeURL = new atlas.service.RouteURL(pipeline);


            // Start and end point input to the routeURL
            let coordinates = [[userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]], [dest.geometry.coordinates[0], dest.geometry.coordinates[1]]];

            // Make a search route request
            routeURL.calculateRouteDirections(atlas.service.Aborter.timeout(10000), coordinates).then((directions) => {
                //Get data features from response
                let data = directions.geojson.getFeatures();
                datasource.add(data);
            });

            map.setCamera({
                bounds: atlas.data.BoundingBox.fromData([userLocation, dest]),
                zoom: 10,
                padding: 15
            });

        }).catch(error => {
            return alert("No drivers nearby please try again later.");
        });
};
