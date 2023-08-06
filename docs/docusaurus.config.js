// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'DressingTools',
  tagline: 'A simple but advanced, non-destructive and automated cabinet system.',
  favicon: 'img/favicon.ico',

  // Set the production url of your site here
  url: 'https://dressingtools.chocopoi.com',
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'poi-vrc', // Usually your GitHub org/user name.
  projectName: 'DressingTools', // Usually your repo name.

  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',

  // Even if you don't use internalization, you can use this field to set useful
  // metadata like html lang. For example, if your site is Chinese, you may want
  // to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl:
            'https://github.com/poi-vrc/DressingTools/docs/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      announcementBar: {
        id: 'docs_for_v2_warning',
        content:
        '⚠️ This documentation is for v2, which is not production-ready yet. You are probably finding <a target="_blank" rel="noopener noreferrer" href="https://github.com/poi-vrc/DressingTools/tree/1.x">v1 here.</a> ⚠️',
        backgroundColor: '#ffd65c',
        textColor: '#000000',
        isCloseable: false,
      },
      navbar: {
        logo: {
          alt: 'DressingTools Logo',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'tutorialSidebar',
            position: 'left',
            label: 'Docs',
          },
          {
            href: 'https://github.com/poi-vrc/DressingTools',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: 'Introduction',
                to: '/docs',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'Discord',
                href: 'https://discord.gg/Gyst8Pr2ay',
              },
              {
                label: 'Twitter',
                href: 'https://twitter.com/chocolapoi',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/poi-vrc/DressingTools',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} chocopoi. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
      },
    }),
};

module.exports = config;
