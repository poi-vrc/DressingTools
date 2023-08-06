import React from 'react';
import clsx from 'clsx';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  image: string;
  description: JSX.Element;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'One-click Dressing',
    image: require('@site/static/img/flaticon/bridal-shower.png').default,
    description: (
      <>
        Add your wearables by either dragging into the avatar or one-click from
        the screen!
      </>
    ),
  },
  {
    title: 'Configure in One-screen',
    image: require('@site/static/img/flaticon/data-collection.png').default,
    description: (
      <>
        Forget about what things to add and where to add. Everything can be
        auto-setup and configured in one-screen!
      </>
    ),
  },
  {
    title: 'Animation Generation',
    image: require('@site/static/img/flaticon/magic-wand.png').default,
    description: (
      <>
        Generates your avatar and wearable toggles and blendshapes animations
        and menus automatically. Making your workflow easier and faster!
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
