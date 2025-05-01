using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.Models.Forum;
using CasaHeights.Models.Forum.Enums;
using CasaHeights.ViewModels.Forum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ForumController> _logger;

        public ForumController(
            AppDbContext context,
            UserManager<Users> userManager,
            ILogger<ForumController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Forum
        public async Task<IActionResult> Index(string category, string tag, string search, int page = 1)
        {
            try
            {
                const int PageSize = 10;

                // Start with base query
                var query = _context.ForumPosts
                    .Include(p => p.Author)
                    .Include(p => p.Categories)
                    .Include(p => p.Tags)
                    .Include(p => p.Comments)
                    .Where(p => !p.IsDeleted && p.Status == PostStatus.Active)
                    .OrderByDescending(p => p.IsPinned)
                    .ThenByDescending(p => p.CreatedAt);

                // Apply filters
                if (!string.IsNullOrEmpty(category))
                {
                    query = (IOrderedQueryable<ForumPost>)query.Where(p => p.Categories.Any(c => c.Name == category));
                }

                if (!string.IsNullOrEmpty(tag))
                {
                    query = (IOrderedQueryable<ForumPost>)query.Where(p => p.Tags.Any(t => t.Name == tag));
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = (IOrderedQueryable<ForumPost>)query.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
                }

                // Get total count for pagination
                var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(total / (double)PageSize);

                // Get paginated results
                var posts = await query
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // Get categories and tags for sidebar
                var categories = await _context.ForumCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var popularTags = await _context.ForumTags
                    .OrderByDescending(t => t.Posts.Count)
                    .Take(10)
                    .ToListAsync();

                var viewModel = new ForumIndexViewModel
                {
                    Posts = posts,
                    Categories = categories,
                    PopularTags = popularTags,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    Category = category,
                    Tag = tag,
                    SearchTerm = search
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading forum index");
                TempData["ErrorMessage"] = "An error occurred while loading the forum.";
                return View(new ForumIndexViewModel());
            }
        }

        // GET: Forum/Post/5
        // Controllers/ForumController.cs
        public async Task<IActionResult> Post(int id)
        {
            var post = await _context.ForumPosts
                .Include(p => p.Author)
                .Include(p => p.Categories)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (post == null)
            {
                return NotFound();
            }

            // Load categories and tags for sidebar
            ViewBag.Categories = await _context.ForumCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            ViewBag.PopularTags = await _context.ForumTags
                .OrderByDescending(t => t.Posts.Count)
                .Take(10)
                .ToListAsync();

            var viewModel = new PostDetailViewModel
            {
                Post = post,
                Comments = post.Comments.Where(c => !c.IsDeleted).ToList()
            };

            return View(viewModel);
        }


        // GET: Forum/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.ForumCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var viewModel = new CreatePostViewModel
            {
                Categories = categories
            };

            return View(viewModel);
        }

        // POST: Forum/Create
        // In ForumController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    
                    var post = new ForumPost
                    {
                        Title = model.Title,
                        Content = model.Content,
                        AuthorId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        Status = PostStatus.Active
                    };

                    // Process categories
                    if (model.SelectedCategories != null)
                    {
                        foreach (var categoryId in model.SelectedCategories)
                        {
                            var category = await _context.ForumCategories.FindAsync(categoryId);
                            if (category != null)
                            {
                                post.Categories.Add(category);
                            }
                        }
                    }

                    // Process tags
                    if (!string.IsNullOrEmpty(model.Tags))
                    {
                        var tagNames = model.Tags.Split(',')
                            .Select(t => t.Trim().ToLower())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .Distinct();

                        foreach (var tagName in tagNames)
                        {
                            var tag = await _context.ForumTags
                                .FirstOrDefaultAsync(t => t.Name == tagName);

                            if (tag == null)
                            {
                                tag = new ForumTag 
                                { 
                                    Name = tagName,
                                    Description = $"Posts tagged with {tagName}" // Default description
                                };
                            }
                            post.Tags.Add(tag);
                        }
                    }

                    _context.ForumPosts.Add(post);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Your post has been created successfully.";
                    return RedirectToAction(nameof(Post), new { id = post.Id });
                }

                // If ModelState is invalid, reload categories
                model.Categories = await _context.ForumCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating forum post");
                ModelState.AddModelError("", "An error occurred while creating the post. Please try again.");
                
                model.Categories = await _context.ForumCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
                return View(model);
            }
        }

        private async Task<(bool Success, int PostId, string ErrorMessage)> CreateForumPostAsync(CreatePostViewModel model, string userId)
        {
            // Start a new transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Beginning transaction to create post: {model.Title}");
                
                var post = new ForumPost
                {
                    Title = model.Title,
                    Content = model.Content,
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = PostStatus.Active,
                };

                // Add post to context (but don't save yet)
                await _context.ForumPosts.AddAsync(post);
                _logger.LogInformation($"Added post to context: {model.Title}");

                // Process categories safely
                if (model.SelectedCategories != null && model.SelectedCategories.Any())
                {
                    _logger.LogInformation($"Processing {model.SelectedCategories.Count} categories");
                    
                    // Create a new collection for categories
                    post.Categories = new List<ForumCategory>();
                    
                    foreach (var categoryId in model.SelectedCategories)
                    {
                        var category = await _context.ForumCategories
                            .FindAsync(categoryId);
                            
                        if (category != null)
                        {
                            post.Categories.Add(category);
                            _logger.LogInformation($"Added category ID {categoryId}");
                        }
                        else
                        {
                            _logger.LogWarning($"Category with ID {categoryId} not found");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No categories selected");
                    post.Categories = new List<ForumCategory>();
                }
                
                // First, save post with categories
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Saved post with ID {post.Id}");
                
                // Process tags separately
                if (!string.IsNullOrEmpty(model.Tags))
                {
                    _logger.LogInformation("Processing tags for post");
                    
                    var tagsList = await ProcessTagsAsync(model.Tags);
                    
                    // Initialize Tags collection if needed
                    post.Tags = post.Tags ?? new List<ForumTag>();
                    
                    // Add processed tags to post
                    foreach (var tag in tagsList)
                    {
                        post.Tags.Add(tag);
                        _logger.LogInformation($"Added tag '{tag.Name}' to post");
                    }
                    
                    // Save changes again with tags
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Saved post with tags");
                }
                else
                {
                    post.Tags = new List<ForumTag>();
                    _logger.LogInformation("No tags provided");
                }

                // Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation($"Transaction committed successfully for post ID {post.Id}");
                
                return (true, post.Id, null);
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, "Database error creating forum post");
                
                string errorMessage = "Error saving to database. ";
                if (dbEx.InnerException != null)
                {
                    _logger.LogError(dbEx.InnerException, "Inner exception details: {0}", dbEx.InnerException.Message);
                    errorMessage += dbEx.InnerException.Message;
                }
                
                return (false, 0, errorMessage);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error in CreateForumPostAsync");
                return (false, 0, "An unexpected error occurred while creating your post. Please try again.");
            }
        }
        
        // New method to handle tag processing separately
        private async Task<List<ForumTag>> ProcessTagsAsync(string tagString)
        {
            var result = new List<ForumTag>();
            
            if (string.IsNullOrEmpty(tagString))
            {
                return result;
            }
            
            var tagNames = tagString.Split(',')
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct()
                .ToList();
                
            _logger.LogInformation($"Processing {tagNames.Count} tags");
            
            foreach (var tagName in tagNames)
            {
                try
                {
                    // Limit tag name length to prevent database errors
                    var safeTagName = tagName.Length > 50 ? tagName.Substring(0, 50) : tagName;
                    var tagDescription = $"Posts tagged with {safeTagName} in the Casa Heights forum.";
                    
                    _logger.LogInformation($"Looking for tag: {safeTagName}");
                    
                    // Try to find existing tag
                    var tag = await _context.ForumTags
                        .FirstOrDefaultAsync(t => t.Name == safeTagName);
                    
                    if (tag == null)
                    {
                        _logger.LogInformation($"Creating new tag: {safeTagName}");
                        // Create and save new tag first
                        tag = new ForumTag 
                        { 
                            Name = safeTagName,
                            Description = tagDescription // Guaranteed non-null description
                        };
                        
                        // Add tag to database and save immediately to get ID
                        _context.ForumTags.Add(tag);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Created and saved new tag with ID {tag.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Found existing tag: {safeTagName}, ID: {tag.Id}");
                        // Check if description needs to be updated
                        if (string.IsNullOrEmpty(tag.Description))
                        {
                            tag.Description = tagDescription;
                            _context.ForumTags.Update(tag);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Updated description for tag {safeTagName}");
                        }
                    }
                    
                    result.Add(tag);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing tag '{tagName}' - skipping this tag");
                    // Continue with other tags
                }
            }
            
            _logger.LogInformation($"Returning {result.Count} processed tags");
            return result;
        }

        // POST: Forum/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Comment content is required.");
            }

            var post = await _context.ForumPosts.FindAsync(postId);
            if (post == null || post.IsDeleted)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var comment = new ForumComment
            {
                Content = content,
                PostId = postId,
                AuthorId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumComments.Add(comment);
            await _context.SaveChangesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    authorName = user.UserName,
                    createdAt = comment.CreatedAt.ToString("MMM dd, yyyy HH:mm"),
                    content = comment.Content
                });
            }

            return RedirectToAction(nameof(Post), new { id = postId });
        }

        // POST: Forum/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(int? postId, int? commentId, string reason)
        {
            if (!postId.HasValue && !commentId.HasValue)
            {
                return BadRequest("Either post or comment ID must be provided.");
            }

            var user = await _userManager.GetUserAsync(User);
            var report = new ForumReport
            {
                PostId = postId,
                CommentId = commentId,
                Reason = reason,
                ReporterId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.ForumReports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your report. Our moderators will review it shortly.";
            
            if (postId.HasValue)
            {
                return RedirectToAction(nameof(Post), new { id = postId.Value });
            }
            
            var comment = await _context.ForumComments.FindAsync(commentId.Value);
            return RedirectToAction(nameof(Post), new { id = comment.PostId });
        }
    }
}