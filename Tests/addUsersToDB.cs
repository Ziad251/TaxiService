using mapsmvcwebapp.Models;
using mapsmvcwebapp.Services;
using mapsmvcwebapp.Utils;
using MongoDB.Driver.GeoJsonObjectModel;

public class addUsersToDB
{
    private FakeUsers _fetchUsers;
    private mongoServiceUser _userDB;

    public addUsersToDB(
    FakeUsers users,
        mongoServiceUser userDB)
    {
        _fetchUsers = users;
        _userDB = userDB;
    }
    public async Task FetchTestUsers()
    {
        var data = _fetchUsers.GetRandomUsers(400);
        Mulberry32 _m32 = new Mulberry32(345345345);

        for (int i = 0; i < 400; i++)
        {
            if (i < 100)
            {
                // give location to each rando
                Coordinates cordinates = new Coordinates();
                cordinates.type = "Point";
                cordinates.longitude = (float)(21.017532 - _m32.Next() / 10000);
                cordinates.latitude = (float)(52.237049 + _m32.Next() / 10000);

                float[] ho = new float[2];
                //longitude
                ho[0] = ((float)(21.017532 - _m32.Next() / 10000));
                //latitude
                ho[1] = ((float)(52.237049 + _m32.Next() / 10000));

                cordinates.geo = GeoJson.Point(GeoJson.Position(ho[0], ho[1]));
                if (data.Result.results[i].location.cordinates == null)
                {
                    data.Result.results[i].location.cordinates = cordinates;
                }
            }

            if (i > 100 && i < 200)
            {
                // give location to each rando
                Coordinates cordinates = new Coordinates();
                cordinates.type = "Point";
                cordinates.longitude = (float)(21.017532 - _m32.Next() / 10000);
                cordinates.latitude = (float)(52.237049 - _m32.Next() / 10000);

                float[] ho = new float[2];
                //longitude
                ho[0] = ((float)(21.017532 - _m32.Next() / 10000));
                //latitude
                ho[1] = ((float)(52.237049 - _m32.Next() / 10000));

                cordinates.geo = GeoJson.Point(GeoJson.Position(ho[0], ho[1]));
                if (data.Result.results[i].location.cordinates == null)
                {
                    data.Result.results[i].location.cordinates = cordinates;
                }
            }

            if (i > 200 && i < 300)
            {
                // give location to each rando
                Coordinates cordinates = new Coordinates();
                cordinates.type = "Point";
                cordinates.longitude = (float)(21.017532 + _m32.Next() / 10000);
                cordinates.latitude = (float)(52.237049 + _m32.Next() / 10000);

                float[] ho = new float[2];
                //longitude
                ho[0] = ((float)(21.017532 + _m32.Next() / 10000));
                //latitude
                ho[1] = ((float)(52.237049 + _m32.Next() / 10000));

                cordinates.geo = GeoJson.Point(GeoJson.Position(ho[0], ho[1]));
                if (data.Result.results[i].location.cordinates == null)
                {
                    data.Result.results[i].location.cordinates = cordinates;
                }
            }
            if (i > 300 && i < 400)
            {
                // give location to each rando
                Coordinates cordinates = new Coordinates();
                cordinates.type = "Point";
                cordinates.longitude = (float)(21.017532 + _m32.Next() / 10000);
                cordinates.latitude = (float)(52.237049 - _m32.Next() / 10000);

                float[] ho = new float[2];
                //longitude
                ho[0] = ((float)(21.017532 + _m32.Next() / 10000));
                //latitude
                ho[1] = ((float)(52.237049 - _m32.Next() / 10000));

                cordinates.geo = GeoJson.Point(GeoJson.Position(ho[0], ho[1]));
                if (data.Result.results[i].location.cordinates == null)
                {
                    data.Result.results[i].location.cordinates = cordinates;
                }
            }

        }


        await _userDB.CreateManyAsync(data.Result.results);

    }
}