let map, datasource, userLocation, finalRoute;

function GetMap() {

    //Initialize a map instance.
    //Center map to user's location
    map = new atlas.Map("myMap", {
        center: [userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]],
        zoom: 12,
        language: 'en-US',
        view: 'Auto',
        // Add your Azure Maps primary subscription key. https://aka.ms/am-primaryKey
        authOptions: {
            authType: 'subscriptionKey',
            subscriptionKey: '_DoxchRePd4oXlazgEE0MmKfwMNdwK_olMeg1Lg5fAs'
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

        console.log(`map layer added. Current user location is ${userLocation}`);
        if (userLocation) {
            console.log(`user location available and added to datasource ${userLocation}`);
            datasource.add(userLocation);
        }
        // if (TestDataSet) {
        //     console.log(`test dataset available and added to datasource`);
        //     datasource.add(TestDataSet);
        // }

        // if (userLocation === undefined) {
        //     console.log("user location unavailable calling update");
        //     var newUserLocation = updateLocation();
        //     console.log(`new user retrieved ${ newUserLocation }`);
        //     if (newUserLocation) {
        //         datasource.add(newUserLocation);
        //         checkNearby();
        //         console.log(`added new location ${ newUserLocation } to ${ datasource }`);
        //     }
        // }


        console.log("end of map ready event");

    });



};

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
    console.log(userLocation);
    return userLocation;
    //createTestData(100);
}

//var random = mulberry32(31142523452);

// function createTestData(n) {

//     console.log(random.next());

//     var test =
//     {
//         type: 'Feature',
//         geometry:
//         {
//             type: 'Point',
//             coordinates: [userLocation.geometry.coordinates[0] - random.next()/10, userLocation.geometry.coordinates[1] + random.next()/10],
//         },
//     }
//     TestDataSet.push(test);
//     if (n > 0) {
//         createTestData(n - 1);
//     }
// };

// function mulberry32(a) {
//     "use strict";
//     return {
//         next: function () {
//             a |= 0; a = a + 0x6D2B79F5 | 0;
//             var t = Math.imul(a ^ a >>> 15, 1 | a);
//             t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t;
//             return ((t ^ t >>> 14) >>> 0) / 4294967296;
//         }
//     }
// };

function updateLocation() {

    console.log(`updateLocation called`);

    navigator.geolocation.watchPosition(pos => {

        const position = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
            ts: pos.timestamp
        };

        var dataBuffer = [];


        setInterval(function () {
            dataBuffer.push(position);
            console.log("new postion added to buffer");
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
        console.log(`userLocation initialized ${userLocation}`);

        // send every 10 seconds if there's new data
        setInterval(function () {
            if (dataBuffer.length) {
                var blob = new Blob([JSON.stringify(dataBuffer)], { type: 'application/json' });
                navigator.sendBeacon('/api/position', blob);
                dataBuffer = [];
            }
            console.log(`data buffer sent and emptied ${dataBuffer}`);
        }, 1200000);

        return userLocation;
    });
};


function buildFeatures(usersLocationData) {
    let positions = usersLocationData.map(user => {
        console.log(`position features built ${positions}`);
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

function InitSecondRoute(des) {
    //Remove any previous results from the map.
    datasource.clear();
    console.log("button clicked");

    // Convert address to coordinates
    key = "_DoxchRePd4oXlazgEE0MmKfwMNdwK_olMeg1Lg5fAs";
    var geoCodingApi = `https://atlas.microsoft.com/search/address/json?subscription-key=${key}&api-version=1.0&language=en-US&`;
    var query = `query=${des}`;
    fetch(geoCodingApi + query, { method: 'get' })
        .then(response => {
            console.log(response)
            return response.json()
        })
        .then(json => {
            console.log(json)
            return json.results;
        })
        .then(data => {

            console.log(data[0]);

            //Create a Feature for finalRoute
            var finalR =
            {
                type: 'Feature',
                geometry:
                {
                    type: 'Point',
                    coordinates: [data[0].position.lon, data[0].position.lat],
                },
            }
            datasource.add(finalR);
            console.log(`final destination is ${finalR}`);

            // Use MapControlCredential to share authentication between a map control and the service module.
            var pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

            //Construct the RouteURL object
            var routeURL = new atlas.service.RouteURL(pipeline);


            // Start and end point input to the routeURL
            var coordinates = [[userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]], [finalR.geometry.coordinates[0], finalR.geometry.coordinates[1]]];
            console.log(`coordinates are ${coordinates}`);

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
        });
};

function checkNearby() {
    //Remove  any previous results from the map.
    datasource.clear();
    console.log("button clicked");
    //Extract GeoJSON feature collection from the response and add it to the datasource
    fetch('/api/nearbyusers', { method: 'get' })
        .then(r => {
            console.log(r)

            return r.json()
        }).then(js => {
            return js
        }).then((data) => {
            console.log(data)

            var destination = data.location.cordinates.geo.Coordinates.Values;

            var dest =
            {
                type: 'Feature',
                geometry:
                {
                    type: 'Point',
                    coordinates: [destination[0], destination[1]],
                },
            }
            datasource.add(dest);
            console.log(`destination is ${destination[0]}, ${destination[1]}`);


            // Use MapControlCredential to share authentication between a map control and the service module.
            var pipeline = atlas.service.MapsURL.newPipeline(new atlas.service.MapControlCredential(map));

            //Construct the RouteURL object
            var routeURL = new atlas.service.RouteURL(pipeline);


            // Start and end point input to the routeURL
            var coordinates = [[userLocation.geometry.coordinates[0], userLocation.geometry.coordinates[1]], [dest.geometry.coordinates[0], dest.geometry.coordinates[1]]];
            console.log(`coordinates are ${coordinates}`);

            // Make a search route request
            routeURL.calculateRouteDirections(atlas.service.Aborter.timeout(10000), coordinates).then((directions) => {
                //Get data features from response
                var data = directions.geojson.getFeatures();
                datasource.add(data);
            });

            map.setCamera({
                bounds: atlas.data.BoundingBox.fromData([userLocation, dest]),
                zoom: 10,
                padding: 15
            });

        }
        );
};
