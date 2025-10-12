// DbInitializer.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Project.Data;
using Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task SeedAllData(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync(); // Ensure database is updated

        // Seed Roles and Admin User (existing logic)
        await SeedRolesAndAdminAsync(serviceProvider);

        // Seed Categories if they don't exist
        if (!context.Categories.Any())
        {
            await SeedCategoriesAsync(context);
        }

        // Seed Users and get their IDs
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        List<ApplicationUser> seededUsers = await SeedUsersAsync(userManager);

        // Seed Items only if they don't already exist
        if (!context.Items.Any())
        {
            await SeedItemsAsync(context, seededUsers);
        }
    }

    // This method remains the same for seeding roles and the main admin
    private static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "Client" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminUser = new ApplicationUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            EmailConfirmed = true,
            Name = "Admin",
            Surname = "User",
            // FIX: Added the PhoneNumber property to satisfy the database constraint
            PhoneNumber = "000-000-0000"
        };

        if (await userManager.FindByEmailAsync(adminUser.Email) == null)
        {
            var createResult = await userManager.CreateAsync(adminUser, "AdminPassword123!");
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        var categories = new Category[]
        {
            new Category { Name = "Animal", IconClass = "fas fa-paw" },
            new Category { Name = "Electronics", IconClass = "fas fa-mobile-alt" },
            new Category { Name = "Jewellery", IconClass = "fas fa-gem" },
            new Category { Name = "Wallets", IconClass = "fas fa-wallet" },
            new Category { Name = "Keys", IconClass = "fas fa-key" },
            new Category { Name = "Bags", IconClass = "fas fa-suitcase" },
            new Category { Name = "Eyewear", IconClass = "fas fa-glasses" },
            new Category { Name = "Clothing", IconClass = "fas fa-tshirt" },
            new Category { Name = "Documents", IconClass = "fas fa-book" },
            new Category { Name = "Bicycles", IconClass = "fas fa-bicycle" },
            new Category { Name = "Musical Instruments", IconClass = "fas fa-music" },
            new Category { Name = "Toys & Games", IconClass = "fas fa-gamepad" }
        };

        foreach (var category in categories)
        {
            context.Categories.Add(category);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds sample ApplicationUsers and returns a list of them.
    /// </summary>
    /// <param name="userManager">The UserManager instance.</param>
    /// <returns>A list of created ApplicationUser objects.</returns>
    private static async Task<List<ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var usersToSeed = new List<ApplicationUser>();

        // Create 5 sample users (you can adjust this number)
        for (int i = 1; i <= 5; i++)
        {
            var user = new ApplicationUser
            {
                UserName = $"client{i}@example.com",
                Email = $"client{i}@example.com",
                EmailConfirmed = true,
                Name = $"Client{i}Name",
                Surname = $"Client{i}Surname",
                // FIX: Add the PhoneNumber property for seeded users
                PhoneNumber = $"000-000-000{i}"
            };

            // Only create if user doesn't already exist
            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                var result = await userManager.CreateAsync(user, $"ClientPassword{i}!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Client");
                    usersToSeed.Add(user); // Add newly created user to the list
                }
            }
            else
            {
                // If user already exists, retrieve it to use its ID for items
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                usersToSeed.Add(existingUser);
            }
        }
        return usersToSeed;
    }


    /// <summary>
    /// Seeds sample Items, distributing them among the provided users.
    /// </summary>
    /// <param name="context">The ApplicationDbContext instance.</param>
    /// <param name="users">A list of ApplicationUser objects to assign as PostedById.</param>
    private static async Task SeedItemsAsync(ApplicationDbContext context, List<ApplicationUser> users)
    {
        if (users == null || !users.Any())
        {
            // Log a warning or throw an exception if no users are available to post items
            return;
        }

        // Cycle through available user IDs for PostedById
        var userIds = users.Select(u => u.Id).ToList();
        var random = new Random();

        var items = new Item[]
        {
            new Item { Title = "Lost iPhone 13", Description = "Found a black iPhone 13 near the library. It's locked.", VerificationQuestion = "What is the background image?", VerificationAnswer = "A picture of a dog", Type = ItemType.Lost, Brand = "Apple", Color = "Black", Location = "Library", DateLost = new DateTime(2025, 8, 10), PhotoPath = "images/iphone.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Car Keys", Description = "Found a set of car keys with a red keychain in the cafeteria.", VerificationQuestion = "What brand is the car?", VerificationAnswer = "Honda", Type = ItemType.Lost, Brand = "Honda", Color = "Red", Location = "Cafeteria", DateLost = new DateTime(2025, 8, 12), PhotoPath = "images/keys.jpg", CategoryId = 5, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Wallet", Description = "Found a brown leather wallet near the main entrance.", VerificationQuestion = "What is the wallet's brand?", VerificationAnswer = "Coach", Type = ItemType.Found, Brand = "Coach", Color = "Brown", Location = "Main Entrance", DateLost = new DateTime(2025, 8, 15), PhotoPath = "images/wallet.jpg", CategoryId = 4, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Student ID Card", Description = "Lost my student ID card. It has a picture of a guy with glasses.", VerificationQuestion = "What is my student ID number?", VerificationAnswer = "12345", Type = ItemType.Lost, Brand = "N/A", Color = "Blue", Location = "Lecture Hall B", DateLost = new DateTime(2025, 8, 13), PhotoPath = "images/id_card.jpg", CategoryId = 9, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Backpack", Description = "Found a black backpack in the parking lot.", VerificationQuestion = "What's in the side pocket?", VerificationAnswer = "A water bottle", Type = ItemType.Found, Brand = "Jansport", Color = "Black", Location = "Parking Lot", DateLost = new DateTime(2025, 8, 11), PhotoPath = "images/backpack.jpg", CategoryId = 6, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Sunglasses", Description = "Lost a pair of sunglasses with a thin metal frame.", VerificationQuestion = "What is the brand of the sunglasses?", VerificationAnswer = "Ray-Ban", Type = ItemType.Lost, Brand = "Ray-Ban", Color = "Silver", Location = "Outdoor Courtyard", DateLost = new DateTime(2025, 8, 14), PhotoPath = "images/sunglasses.jpg", CategoryId = 7, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Umbrella", Description = "Found a large, black umbrella in the foyer.", VerificationQuestion = "What logo is on the handle?", VerificationAnswer = "A small dog", Type = ItemType.Found, Brand = "Totes", Color = "Black", Location = "Foyer", DateLost = new DateTime(2025, 8, 16), PhotoPath = "images/umbrella.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Laptop Charger", Description = "Lost a MacBook charger in the main computer lab.", VerificationQuestion = "What is the serial number on the charger?", VerificationAnswer = "ABC123XYZ", Type = ItemType.Lost, Brand = "Apple", Color = "White", Location = "Computer Lab", DateLost = new DateTime(2025, 8, 17), PhotoPath = "images/charger.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Small Key", Description = "Found a single, small silver key on a table in the cafe.", VerificationQuestion = "What is the number stamped on the key?", VerificationAnswer = "789", Type = ItemType.Found, Brand = "N/A", Color = "Silver", Location = "Cafe", DateLost = new DateTime(2025, 8, 18), PhotoPath = "images/small_key.jpg", CategoryId = 5, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Gym Towel", Description = "Lost a blue and white gym towel in the locker room.", VerificationQuestion = "What name is embroidered on the corner?", VerificationAnswer = "Alex", Type = ItemType.Lost, Brand = "Nike", Color = "Blue", Location = "Gym Locker Room", DateLost = new DateTime(2025, 8, 19), PhotoPath = "images/towel.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Leather Belt", Description = "Found a brown leather belt with a brass buckle.", VerificationQuestion = "What brand is stamped on the inside?", VerificationAnswer = "Fossil", Type = ItemType.Found, Brand = "Fossil", Color = "Brown", Location = "Student Union", DateLost = new DateTime(2025, 8, 20), PhotoPath = "images/belt.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Headphones", Description = "Lost a pair of black over-ear headphones.", VerificationQuestion = "What is the name of the headphones model?", VerificationAnswer = "WH-1000XM5", Type = ItemType.Lost, Brand = "Sony", Color = "Black", Location = "Music Room", DateLost = new DateTime(2025, 8, 21), PhotoPath = "images/headphones.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found USB Drive", Description = "Found a small USB flash drive, red in color.", VerificationQuestion = "What is written on the side of the drive?", VerificationAnswer = "Final Project", Type = ItemType.Found, Brand = "Kingston", Color = "Red", Location = "Computer Lab", DateLost = new DateTime(2025, 8, 22), PhotoPath = "images/usb_drive.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Water Bottle", Description = "Lost a green water bottle with a sticker on it.", VerificationQuestion = "What is the sticker of?", VerificationAnswer = "A sunflower", Type = ItemType.Lost, Brand = "Hydro Flask", Color = "Green", Location = "Gym", DateLost = new DateTime(2025, 8, 23), PhotoPath = "images/water_bottle.jpg", CategoryId = 6, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Sweater", Description = "Found a gray hooded sweater left on a bench.", VerificationQuestion = "What is the small logo on the chest?", VerificationAnswer = "A polo player", Type = ItemType.Found, Brand = "Polo Ralph Lauren", Color = "Gray", Location = "Park Bench", DateLost = new DateTime(2025, 8, 24), PhotoPath = "images/sweater.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Watch", Description = "Lost a digital watch with a black strap.", VerificationQuestion = "What is the time currently displayed on the watch?", VerificationAnswer = "10:30", Type = ItemType.Lost, Brand = "Casio", Color = "Black", Location = "Swimming Pool", DateLost = new DateTime(2025, 8, 25), PhotoPath = "images/watch.jpg", CategoryId = 3, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Book", Description = "Found a textbook for a history class.", VerificationQuestion = "What is the full title of the book?", VerificationAnswer = "A History of the Modern World", Type = ItemType.Found, Brand = "N/A", Color = "Multicolor", Location = "History Department", DateLost = new DateTime(2025, 8, 26), PhotoPath = "images/history_book.jpg", CategoryId = 9, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Ring", Description = "Lost a silver ring with a small sapphire gem.", VerificationQuestion = "What date is engraved on the inside?", VerificationAnswer = "04/22/2022", Type = ItemType.Lost, Brand = "N/A", Color = "Silver", Location = "Bathroom", DateLost = new DateTime(2025, 8, 27), PhotoPath = "images/ring.jpg", CategoryId = 3, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Charging Cable", Description = "Found a white phone charging cable, type-C.", VerificationQuestion = "Is it braided or plastic?", VerificationAnswer = "Plastic", Type = ItemType.Found, Brand = "Samsung", Color = "White", Location = "Classroom 101", DateLost = new DateTime(2025, 8, 28), PhotoPath = "images/cable.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Black Jacket", Description = "Lost a black leather jacket. It has a zipper on the sleeve.", VerificationQuestion = "What kind of lining does it have?", VerificationAnswer = "Red silk", Type = ItemType.Lost, Brand = "Zara", Color = "Black", Location = "Auditorium", DateLost = new DateTime(2025, 8, 29), PhotoPath = "images/jacket.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Keys with Tag", Description = "Found a set of keys with a small blue tag on it.", VerificationQuestion = "What is the number written on the tag?", VerificationAnswer = "153", Type = ItemType.Found, Brand = "N/A", Color = "Blue", Location = "Lost and Found Office", DateLost = new DateTime(2025, 8, 30), PhotoPath = "images/keys_tag.jpg", CategoryId = 5, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Earbuds", Description = "Lost my wireless earbuds in a white charging case.", VerificationQuestion = "What is the unique scuff mark on the bottom?", VerificationAnswer = "A tiny star", Type = ItemType.Lost, Brand = "Apple", Color = "White", Location = "Gym", DateLost = new DateTime(2025, 8, 31), PhotoPath = "images/earbuds.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Brown Hat", Description = "Found a soft brown felt hat.", VerificationQuestion = "What's the feather color on the side?", VerificationAnswer = "Blue", Type = ItemType.Found, Brand = "Stetson", Color = "Brown", Location = "The Quad", DateLost = new DateTime(2025, 9, 1), PhotoPath = "images/hat.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Gloves", Description = "Lost a pair of black leather gloves.", VerificationQuestion = "Are they lined with anything?", VerificationAnswer = "Wool", Type = ItemType.Lost, Brand = "North Face", Color = "Black", Location = "Bus Stop", DateLost = new DateTime(2025, 9, 2), PhotoPath = "images/gloves.jpg", CategoryId = 8, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found ID Card", Description = "Found a university ID card on the sidewalk.", VerificationQuestion = "What is the year of graduation?", VerificationAnswer = "2026", Type = ItemType.Found, Brand = "N/A", Color = "Green", Location = "Sidewalk", DateLost = new DateTime(2025, 9, 3), PhotoPath = "images/student_id.jpg", CategoryId = 9, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Backpack", Description = "Lost a red backpack with a front pocket.", VerificationQuestion = "What brand is the logo on the front?", VerificationAnswer = "Adidas", Type = ItemType.Lost, Brand = "Adidas", Color = "Red", Location = "Library", DateLost = new DateTime(2025, 9, 4), PhotoPath = "images/red_backpack.jpg", CategoryId = 6, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Silver Ring", Description = "Found a simple silver band ring.", VerificationQuestion = "Is there any engraving on the inside?", VerificationAnswer = "Yes, 'J&S'", Type = ItemType.Found, Brand = "N/A", Color = "Silver", Location = "Gym", DateLost = new DateTime(2025, 9, 5), PhotoPath = "images/silver_ring.jpg", CategoryId = 3, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost House Keys", Description = "Lost a large set of house keys with a bottle opener.", VerificationQuestion = "What color is the bottle opener?", VerificationAnswer = "Blue", Type = ItemType.Lost, Brand = "N/A", Color = "Silver", Location = "Cafeteria", DateLost = new DateTime(2025, 9, 6), PhotoPath = "images/house_keys.jpg", CategoryId = 5, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Found Passport", Description = "Found a passport. The cover is dark blue.", VerificationQuestion = "What country is the passport from?", VerificationAnswer = "United States", Type = ItemType.Found, Brand = "N/A", Color = "Blue", Location = "Airport", DateLost = new DateTime(2025, 9, 7), PhotoPath = "images/passport.jpg", CategoryId = 9, PostedById = userIds[random.Next(userIds.Count)] },
            new Item { Title = "Lost Airpods Case", Description = "Lost an Airpods charging case without the buds inside.", VerificationQuestion = "Is there any damage on the case?", VerificationAnswer = "A small crack on the lid", Type = ItemType.Lost, Brand = "Apple", Color = "White", Location = "Study Hall", DateLost = new DateTime(2025, 9, 8), PhotoPath = "images/airpods_case.jpg", CategoryId = 2, PostedById = userIds[random.Next(userIds.Count)] }
        };

        foreach (var item in items)
        {
            context.Items.Add(item);
        }
        await context.SaveChangesAsync();
    }
}