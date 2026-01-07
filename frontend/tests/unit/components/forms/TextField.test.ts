import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import TextField from '@/components/forms/TextField.vue';

describe('TextField Component', () => {
  it('should render with label', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
      },
    });

    expect(wrapper.find('.label').text()).toBe('Email');
  });

  it('should render input with correct type', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Password',
        modelValue: '',
        type: 'password',
      },
    });

    const input = wrapper.find('.input') as any;
    expect(input.attributes('type')).toBe('password');
  });

  it('should default to text type when not specified', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Name',
        modelValue: '',
      },
    });

    const input = wrapper.find('.input') as any;
    expect(input.attributes('type')).toBe('text');
  });

  it('should display modelValue in input', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: 'test@example.com',
      },
    });

    const input = wrapper.find('.input') as any;
    expect(input.element.value).toBe('test@example.com');
  });

  it('should emit update:modelValue on input change', async () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
      },
    });

    const input = wrapper.find('.input');
    await input.setValue('new@example.com');

    expect(wrapper.emitted('update:modelValue')).toHaveLength(1);
    expect(wrapper.emitted('update:modelValue')![0]).toEqual(['new@example.com']);
  });

  it('should display help text when provided', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
        help: 'Enter your email address',
      },
    });

    expect(wrapper.find('.help').exists()).toBe(true);
    expect(wrapper.find('.help').text()).toBe('Enter your email address');
  });

  it('should not display help text when not provided', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
      },
    });

    expect(wrapper.find('.help').exists()).toBe(false);
  });

  it('should display error text when provided', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
        error: 'Email is required',
      },
    });

    expect(wrapper.find('.error').exists()).toBe(true);
    expect(wrapper.find('.error').text()).toBe('Email is required');
  });

  it('should not display error text when not provided', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
      },
    });

    expect(wrapper.find('.error').exists()).toBe(false);
  });

  it('should handle multiple input changes', async () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
      },
    });

    const input = wrapper.find('.input');

    await input.setValue('first@example.com');
    await input.setValue('second@example.com');
    await input.setValue('third@example.com');

    expect(wrapper.emitted('update:modelValue')).toHaveLength(3);
    expect(wrapper.emitted('update:modelValue')![2]).toEqual(['third@example.com']);
  });

  it('should support email type', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
        type: 'email',
      },
    });

    const input = wrapper.find('.input') as any;
    expect(input.attributes('type')).toBe('email');
  });

  it('should display both help and error text', () => {
    const wrapper = mount(TextField, {
      props: {
        label: 'Email',
        modelValue: '',
        help: 'Enter a valid email',
        error: 'Email is invalid',
      },
    });

    expect(wrapper.find('.help').exists()).toBe(true);
    expect(wrapper.find('.error').exists()).toBe(true);
    expect(wrapper.find('.help').text()).toBe('Enter a valid email');
    expect(wrapper.find('.error').text()).toBe('Email is invalid');
  });
});
