import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';
import Translate, { translate } from '@docusaurus/Translate';

type FeatureItem = {
  title: string;
  image: string;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: translate({ message: 'One-click Dressing' }),
    image: require('@site/static/img/flaticon/bridal-shower.png').default,
    description: (
      <>
        <Translate>Add your wearables by either dragging into the avatar or one-click from
        the screen!</Translate>
      </>
    ),
  },
  {
    title: translate({ message: 'Configure in One-screen' }),
    image: require('@site/static/img/flaticon/data-collection.png').default,
    description: (
      <>
        <Translate>Forget about what things to add and where to add. Everything can be
        auto-setup and configured in one-screen!</Translate>
      </>
    ),
  },
  {
    title: translate({ message: 'Animation Generation' }),
    image: require('@site/static/img/flaticon/magic-wand.png').default,
    description: (
      <>
        <Translate>Generates your avatar and wearable toggles and blendshapes animations
        and menus automatically. Making your workflow easier and faster!</Translate>
      </>
    ),
  },
];

function Feature({ title, image, description }: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <img className={styles.featureSvg} src={image} />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): JSX.Element {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
