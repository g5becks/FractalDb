/**
 * Integration tests for multi-field text search.
 *
 * @remarks
 * These tests verify that the search option works correctly through
 * the full StrataDB API with real database operations.
 */

import { afterEach, beforeEach, describe, expect, it } from "bun:test"
import { createSchema, type Document, Strata } from "../../src/index.js"

// ============================================================================
// Test Document Types
// ============================================================================

/**
 * Article document for testing text search.
 */
type Article = Document<{
  title: string
  content: string
  author: string
  category: string
  tags: string[]
  metadata: {
    summary: string
    keywords: string
  }
}>

// ============================================================================
// Test Setup
// ============================================================================

describe("Text Search Integration", () => {
  let db: Strata

  const createArticleSchema = () =>
    createSchema<Article>()
      .field("title", { type: "TEXT", indexed: true })
      .field("content", { type: "TEXT", indexed: false })
      .field("author", { type: "TEXT", indexed: true })
      .field("category", { type: "TEXT", indexed: true })
      .build()

  beforeEach(async () => {
    db = new Strata({ database: ":memory:" })
    const articles = db.collection("articles", createArticleSchema())

    // Seed test data
    await articles.insertMany([
      {
        title: "Introduction to TypeScript",
        content: "TypeScript is a strongly typed programming language...",
        author: "Alice Johnson",
        category: "programming",
        tags: ["typescript", "javascript", "types"],
        metadata: {
          summary: "Learn the basics of TypeScript",
          keywords: "typescript, types, javascript",
        },
      },
      {
        title: "Advanced React Patterns",
        content: "React is a popular JavaScript library for building UIs...",
        author: "Bob Smith",
        category: "programming",
        tags: ["react", "javascript", "patterns"],
        metadata: {
          summary: "Deep dive into React patterns",
          keywords: "react, hooks, components",
        },
      },
      {
        title: "Database Design Fundamentals",
        content:
          "Good database design is essential for application performance...",
        author: "Alice Johnson",
        category: "databases",
        tags: ["sql", "nosql", "design"],
        metadata: {
          summary: "Learn database design principles",
          keywords: "database, sql, normalization",
        },
      },
      {
        title: "SQLite Performance Tips",
        content:
          "SQLite is a lightweight database engine perfect for embedded apps...",
        author: "Charlie Brown",
        category: "databases",
        tags: ["sqlite", "performance", "optimization"],
        metadata: {
          summary: "Optimize your SQLite queries",
          keywords: "sqlite, performance, indexing",
        },
      },
      {
        title: "Building APIs with Node.js",
        content: "Node.js makes it easy to build fast and scalable APIs...",
        author: "Diana Prince",
        category: "backend",
        tags: ["nodejs", "api", "javascript"],
        metadata: {
          summary: "Create REST APIs with Node",
          keywords: "nodejs, express, api",
        },
      },
    ])
  })

  afterEach(() => {
    db.close()
  })

  // ==========================================================================
  // BASIC SEARCH TESTS
  // ==========================================================================

  describe("basic text search", () => {
    it("should search across a single indexed field", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "typescript",
            fields: ["title"],
          },
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Introduction to TypeScript")
    })

    it("should search across multiple indexed fields", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "alice",
            fields: ["title", "author"],
          },
        }
      )

      expect(results).toHaveLength(2) // Both articles by Alice Johnson
      for (const article of results) {
        expect(article.author).toBe("Alice Johnson")
      }
    })

    it("should search across non-indexed fields", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "programming language",
            fields: ["content"],
          },
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Introduction to TypeScript")
    })

    it("should search with OR logic across fields", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // "sqlite" appears in title "SQLite Performance Tips"
      // "database" appears in "Database Design Fundamentals" title
      const results = await articles.find(
        {},
        {
          search: {
            text: "sqlite",
            fields: ["title", "content"],
          },
        }
      )

      // Should find articles where "sqlite" is in title OR content
      expect(results.length).toBeGreaterThanOrEqual(1)
      expect(results.some((a) => a.title === "SQLite Performance Tips")).toBe(
        true
      )
    })
  })

  // ==========================================================================
  // CASE SENSITIVITY TESTS
  // ==========================================================================

  describe("case sensitivity", () => {
    it("should be case-insensitive by default", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const lowerResults = await articles.find(
        {},
        {
          search: {
            text: "typescript",
            fields: ["title"],
          },
        }
      )

      const upperResults = await articles.find(
        {},
        {
          search: {
            text: "TYPESCRIPT",
            fields: ["title"],
          },
        }
      )

      const mixedResults = await articles.find(
        {},
        {
          search: {
            text: "TypeScript",
            fields: ["title"],
          },
        }
      )

      expect(lowerResults).toHaveLength(1)
      expect(upperResults).toHaveLength(1)
      expect(mixedResults).toHaveLength(1)
    })

    it("should accept caseSensitive option", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Note: SQLite's LIKE operator is case-insensitive for ASCII by default.
      // The caseSensitive option removes COLLATE NOCASE, but SQLite LIKE
      // is still case-insensitive for basic ASCII characters.
      // This test verifies the option is accepted and doesn't cause errors.
      const results = await articles.find(
        {},
        {
          search: {
            text: "TypeScript",
            fields: ["content"],
            caseSensitive: true,
          },
        }
      )

      // Should find the TypeScript article
      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Introduction to TypeScript")
    })
  })

  // ==========================================================================
  // COMBINED FILTER AND SEARCH
  // ==========================================================================

  describe("combined filter and search", () => {
    it("should combine search with equality filter", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for "programming" in content within programming category
      const results = await articles.find(
        { category: "programming" },
        {
          search: {
            text: "programming",
            fields: ["content"],
          },
        }
      )

      expect(results).toHaveLength(1) // Only TypeScript article has "programming" in content
      expect(results[0].category).toBe("programming")
    })

    it("should combine search with comparison operators", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for "database" excluding certain categories
      const results = await articles.find(
        { category: { $ne: "programming" } },
        {
          search: {
            text: "database",
            fields: ["title", "content"],
          },
        }
      )

      // Should find database articles not in programming
      for (const article of results) {
        expect(article.category).not.toBe("programming")
      }
    })

    it("should combine search with $or filter", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {
          $or: [{ category: "databases" }, { author: "Diana Prince" }],
        },
        {
          search: {
            text: "api",
            fields: ["title", "content"],
          },
        }
      )

      // Should find "Building APIs" by Diana Prince
      expect(results.length).toBeGreaterThanOrEqual(1)
    })
  })

  // ==========================================================================
  // SEARCH WITH OPTIONS
  // ==========================================================================

  describe("search with query options", () => {
    it("should combine search with sort", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "alice",
            fields: ["author"],
          },
          sort: { title: 1 },
        }
      )

      expect(results).toHaveLength(2)
      // Sorted alphabetically by title
      expect(results[0].title).toBe("Database Design Fundamentals")
      expect(results[1].title).toBe("Introduction to TypeScript")
    })

    it("should combine search with limit", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for "database" which appears in 2 article titles
      const results = await articles.find(
        {},
        {
          search: {
            text: "database",
            fields: ["content", "title"],
          },
          limit: 1,
        }
      )

      expect(results).toHaveLength(1)
    })

    it("should combine search with select projection", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "react",
            fields: ["title", "content"],
          },
          select: ["title", "author"],
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Advanced React Patterns")
      expect(results[0].author).toBe("Bob Smith")
      expect("content" in results[0]).toBe(false)
      expect("category" in results[0]).toBe(false)
    })

    it("should combine search with omit projection", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "sqlite",
            fields: ["title"],
          },
          omit: ["content", "metadata"],
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("SQLite Performance Tips")
      expect("content" in results[0]).toBe(false)
      expect("metadata" in results[0]).toBe(false)
    })
  })

  // ==========================================================================
  // NESTED FIELD SEARCH
  // ==========================================================================

  describe("nested field search", () => {
    it("should search in nested fields using dot notation", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "hooks",
            fields: ["metadata.keywords"],
          },
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Advanced React Patterns")
    })

    it("should search across mixed top-level and nested fields", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for "optimization" in title and metadata.summary
      const results = await articles.find(
        {},
        {
          search: {
            text: "optimize",
            fields: ["title", "metadata.summary"],
          },
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("SQLite Performance Tips")
    })
  })

  // ==========================================================================
  // EDGE CASES
  // ==========================================================================

  describe("edge cases", () => {
    it("should return all documents when search text is empty", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "",
            fields: ["title"],
          },
        }
      )

      // Empty search text should not filter
      expect(results).toHaveLength(5)
    })

    it("should return all documents when fields array is empty", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "typescript",
            fields: [],
          },
        }
      )

      // No fields to search should not filter
      expect(results).toHaveLength(5)
    })

    it("should return empty array when no matches found", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.find(
        {},
        {
          search: {
            text: "xyznonexistent123",
            fields: ["title", "content", "author"],
          },
        }
      )

      expect(results).toHaveLength(0)
    })

    it("should handle special characters in search text", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for text with special SQL characters
      const results = await articles.find(
        {},
        {
          search: {
            text: "Node.js",
            fields: ["title", "content"],
          },
        }
      )

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Building APIs with Node.js")
    })
  })

  // ==========================================================================
  // FINDONE WITH SEARCH
  // ==========================================================================

  describe("findOne with search", () => {
    it("should apply search to findOne", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const result = await articles.findOne(
        {},
        {
          search: {
            text: "react",
            fields: ["title"],
          },
        }
      )

      expect(result).not.toBeNull()
      expect(result?.title).toBe("Advanced React Patterns")
    })

    it("should return null when findOne with search has no match", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const result = await articles.findOne(
        {},
        {
          search: {
            text: "nonexistent",
            fields: ["title"],
          },
        }
      )

      expect(result).toBeNull()
    })
  })

  // ==========================================================================
  // DEDICATED SEARCH METHOD
  // ==========================================================================

  describe("dedicated search() method", () => {
    it("should search with simple API", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("typescript", ["title", "content"])

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Introduction to TypeScript")
    })

    it("should search with filter option", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Search for "database" but only in the databases category
      const results = await articles.search("database", ["title", "content"], {
        filter: { category: "databases" },
      })

      expect(results.length).toBeGreaterThanOrEqual(1)
      for (const article of results) {
        expect(article.category).toBe("databases")
      }
    })

    it("should search with sort and limit options", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("alice", ["author"], {
        sort: { title: 1 },
        limit: 1,
      })

      expect(results).toHaveLength(1)
      expect(results[0].author).toBe("Alice Johnson")
    })

    it("should search with select projection", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("react", ["title"], {
        select: ["title", "author"],
      })

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Advanced React Patterns")
      expect("content" in results[0]).toBe(false)
    })

    it("should search with omit projection", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("sqlite", ["title"], {
        omit: ["content", "metadata"],
      })

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("SQLite Performance Tips")
      expect("content" in results[0]).toBe(false)
      expect("metadata" in results[0]).toBe(false)
    })

    it("should return empty array when no matches", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("xyznonexistent", [
        "title",
        "content",
      ])

      expect(results).toHaveLength(0)
    })

    it("should search in nested fields", async () => {
      const articles = db.collection("articles", createArticleSchema())

      const results = await articles.search("hooks", ["metadata.keywords"])

      expect(results).toHaveLength(1)
      expect(results[0].title).toBe("Advanced React Patterns")
    })

    it("should accept caseSensitive option", async () => {
      const articles = db.collection("articles", createArticleSchema())

      // Note: SQLite LIKE is case-insensitive for ASCII by default,
      // so this test verifies the option is accepted without errors
      const results = await articles.search("TypeScript", ["content"], {
        caseSensitive: true,
      })

      expect(results).toHaveLength(1)
    })
  })
})
