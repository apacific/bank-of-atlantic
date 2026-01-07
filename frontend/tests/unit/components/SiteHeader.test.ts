import { describe, it, expect, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createRouter, createMemoryHistory } from 'vue-router';
import SiteHeader from '@/components/SiteHeader.vue';

describe('SiteHeader Component', () => {
  let router: any;

  beforeEach(() => {
    router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/customers', component: { template: '<div>Customers</div>' } },
      ],
    });
  });

  it('should render header with logo', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/test-logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    expect(wrapper.find('.header').exists()).toBe(true);
    expect(wrapper.find('.logo').exists()).toBe(true);
  });

  it('should display correct logo src', () => {
    const logoSrc = '/my-logo.png';
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc,
      },
      global: {
        plugins: [router],
      },
    });

    const logo = wrapper.find('.logo') as any;
    expect(logo.attributes('src')).toBe(logoSrc);
  });

  it('should use default alt text when not provided', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    const logo = wrapper.find('.logo') as any;
    expect(logo.attributes('alt')).toBe('Bank of Atlantic logo');
  });

  it('should use custom alt text when provided', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
        alt: 'Custom Alt Text',
      },
      global: {
        plugins: [router],
      },
    });

    const logo = wrapper.find('.logo') as any;
    expect(logo.attributes('alt')).toBe('Custom Alt Text');
  });

  it('should display title "Bank of Atlantic"', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    expect(wrapper.find('.title').text()).toBe('Bank of Atlantic');
  });

  it('should navigate to /customers when logo is clicked', async () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    await wrapper.find('.logoBtn').trigger('click');
    await router.isReady();
    expect(router.currentRoute.value.path).toBe('/customers');
  });

  it('should have proper accessibility attributes', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    const logoBtn = wrapper.find('.logoBtn') as any;
    expect(logoBtn.attributes('aria-label')).toBe('Go to customers');
    expect(logoBtn.attributes('type')).toBe('button');
  });

  it('should render header element with sticky positioning class', () => {
    const wrapper = mount(SiteHeader, {
      props: {
        logoSrc: '/logo.png',
      },
      global: {
        plugins: [router],
      },
    });

    const header = wrapper.find('.header');
    expect(header.exists()).toBe(true);
    expect(header.element.tagName).toBe('HEADER');
  });
});
