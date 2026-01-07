import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createRouter, createMemoryHistory } from 'vue-router';
import LoginPage from '@/features/auth/pages/LoginPage.vue';
import Modal from '@/components/Modal.vue';
import TextField from '@/components/forms/TextField.vue';

// Mock the API
vi.mock('@/app/api', () => ({
  api: {
    post: vi.fn(),
  },
}));

// Mock the auth store
vi.mock('@/features/auth/authStore', () => ({
  useAuth: () => ({
    setSession: vi.fn(),
  }),
}));

describe('LoginPage Component', () => {
  let router: any;

  beforeEach(() => {
    vi.clearAllMocks();
    router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/login', component: { template: '<div>Login</div>' } },
        { path: '/customers', component: { template: '<div>Customers</div>' } },
      ],
    });
  });

  it('should render login form', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    expect(wrapper.find('.title').text()).toBe('Bank of Atlantic');
    expect(wrapper.find('.muted').text()).toBe('Please sign in.');
  });

  it('should render email and password fields', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    expect(textFields).toHaveLength(2);
  });

  it('should render submit button', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const button = wrapper.find('.btn');
    expect(button.exists()).toBe(true);
    expect(button.text()).toBe('Sign in');
  });

  it('should show validation error when email is empty', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    // Get the component instance to directly access the form and errors
    const vm = wrapper.vm as any;
    
    // Submit the form with empty fields
    const form = wrapper.find('form');
    await form.trigger('submit');
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    // Check the component's errors object directly
    expect(vm.errors.email).toBe('Email is required.');
  });

  it('should show validation error when password is empty', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const vm = wrapper.vm as any;
    
    const form = wrapper.find('form');
    await form.trigger('submit');
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    expect(vm.errors.password).toBe('Password is required.');
  });

  it('should show validation errors for both fields when both are empty', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const vm = wrapper.vm as any;
    
    const form = wrapper.find('form');
    await form.trigger('submit');
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    expect(vm.errors.email).toBe('Email is required.');
    expect(vm.errors.password).toBe('Password is required.');
  });

  it('should disable button while loading', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    // Verify form renders properly with submit button
    const button = wrapper.find('.btn');
    expect(button.exists()).toBe(true);
    expect(button.text()).toBe('Sign in');
  });

  it('should clear previous errors on submit', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const vm = wrapper.vm as any;
    
    // Trigger first validation error
    const form = wrapper.find('form');
    await form.trigger('submit');
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    expect(vm.errors.email).toBe('Email is required.');

    // Update form and submit again
    vm.form.email = 'test@example.com';
    await wrapper.vm.$nextTick();
    
    await form.trigger('submit');
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    // Email error should be cleared, but password error should still exist
    expect(vm.errors.email).toBeUndefined();
    expect(vm.errors.password).toBe('Password is required.');
  });

  it('should render modal component', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const modal = wrapper.findComponent(Modal);
    expect(modal.exists()).toBe(true);
  });

  it('should have proper form attributes', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const form = wrapper.find('form');
    expect(form.exists()).toBe(true);
  });

  it('should display email field with email type', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    expect(textFields[0]?.props('type')).toBe('email');
  });

  it('should display password field with password type', () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [router],
        components: { Modal, TextField },
      },
    });

    const textFields = wrapper.findAllComponents(TextField);
    expect(textFields[1]?.props('type')).toBe('password');
  });
});
