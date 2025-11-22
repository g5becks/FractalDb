import { defineConfig } from "vitepress"
import typedocSidebar from "../api/typedoc-sidebar.json"

export default defineConfig({
  title: "StrataDB",
  description: "Type-safe document database built on bun:sqlite",
  base: "/stratadb/",

  themeConfig: {
    nav: [
      { text: "Guide", link: "/guide/" },
      { text: "API", link: "/api/" },
    ],

    sidebar: {
      "/guide/": [
        {
          text: "Introduction",
          items: [
            { text: "What is StrataDB?", link: "/guide/" },
            { text: "Getting Started", link: "/guide/getting-started" },
            { text: "Installation", link: "/guide/installation" },
          ],
        },
        {
          text: "Core Concepts",
          items: [
            { text: "Documents", link: "/guide/documents" },
            { text: "Schemas", link: "/guide/schemas" },
            { text: "Collections", link: "/guide/collections" },
            { text: "Queries", link: "/guide/queries" },
          ],
        },
        {
          text: "Advanced",
          items: [
            { text: "Transactions", link: "/guide/transactions" },
            { text: "Validation", link: "/guide/validation" },
            { text: "Indexes", link: "/guide/indexes" },
          ],
        },
        {
          text: "Help & Support",
          items: [
            { text: "FAQ", link: "/guide/faq" },
            { text: "Troubleshooting", link: "/guide/troubleshooting" },
            { text: "Migration", link: "/guide/migration" },
            { text: "Security", link: "/guide/security" },
            { text: "Testing", link: "/guide/testing" },
            { text: "Quick Reference", link: "/guide/quick-reference" },
            { text: "Performance", link: "/guide/performance" },
          ],
        },
      ],
      "/api/": typedocSidebar,
    },

    socialLinks: [
      { icon: "github", link: "https://github.com/takinprofit/stratadb" },
    ],

    footer: {
      message: "Released under the MIT License.",
    },

    search: {
      provider: "local",
    },
  },
})
