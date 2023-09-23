import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import Logo from '@site/static/img/logo-white.svg';
import Translate, { translate } from '@docusaurus/Translate';

import styles from './index.module.css';

function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <h1 className="hero__title">
          <Logo className={styles.logo} xlinkTitle='DressingTools' />
        </h1>
        <p className="hero__subtitle">{translate({
                message: siteConfig.tagline
            })}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            target="_self"
            to="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json">
            <Translate>Install with VCC</Translate> ❤️
          </Link>
          <Link
            className="button button--secondary button--lg"
            to="/docs">
            <Translate>Guides</Translate> 📚
          </Link>
        </div>
      </div>
    </header >
  );
}

export default function Home(): JSX.Element {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`DressingTools`}
      description={translate({
        message: "A simple but advanced, non-destructive cabinet system"
      })}>
      < HomepageHeader />
      <main>
        <HomepageFeatures />
      </main>
    </Layout >
  );
}
