using AsyncProductAPI.Data;
using AsyncProductAPI.Dtos;
using AsyncProductAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=RequestDB.db"));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("api/v1/products", async (AppDbContext context, ListingRequest listingRequest) =>
{
    if (listingRequest is null)
        return Results.BadRequest();

    listingRequest.RequestStatus = "ACCEPT";
    listingRequest.EstimatedCompletionTime = "2023-02-13:14:00:00";

    await context.ListingRequests.AddAsync(listingRequest);
    await context.SaveChangesAsync();

    return Results.Accepted($"api/v1/productstatus/{listingRequest.RequestId}", listingRequest);
});

app.MapGet("api/v1/productstatus/{requestId}", (AppDbContext context, string requestId) =>
{
    var listingRequest = context.ListingRequests.FirstOrDefault(lr => lr.RequestId == requestId);

    if (listingRequest is null)
        return Results.NotFound();

    var listingStatus = new ListingStatus()
    {
        RequestStatus = listingRequest.RequestStatus,
        ResourceUrl = String.Empty,
    };

    if (listingRequest.RequestStatus!.ToUpper() == "COMPLETE")
    {
        listingStatus.ResourceUrl = $"api/v1/products/{Guid.NewGuid()}";

        return Results.Redirect($"http://localhost:5263/{listingStatus.ResourceUrl}");
    }

    listingStatus.EstimatedCompletionTime = "2023-02-13:15:00:00";
    return Results.Ok(listingStatus);
});


app.MapGet("api/v1/products/{requestId}", (string requestId) =>
{
    return Results.Ok("Returning the final result");
});

app.Run();